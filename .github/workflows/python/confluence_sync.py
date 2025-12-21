import base64
import json
import os
import re
import urllib.request
from datetime import datetime, timezone
from html.parser import HTMLParser


def get_env(name: str) -> str:
    value = os.environ.get(name, "")
    if not value:
        raise SystemExit(f"Missing required environment variable: {name}")
    return value


def sanitize_path_part(value: str) -> str:
    cleaned = re.sub(r'[\\/:*?"<>|]', "-", value).strip()
    return cleaned or "Untitled"


class MinimalMarkdownParser(HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self.parts: list[str] = []

    def handle_starttag(self, tag, attrs):
        if tag in {"p", "div", "br", "h1", "h2", "h3", "h4", "h5", "h6"}:
            self.parts.append("\n")
        if tag == "li":
            self.parts.append("\n- ")

    def handle_endtag(self, tag):
        if tag in {"p", "div", "li", "h1", "h2", "h3", "h4", "h5", "h6"}:
            self.parts.append("\n")

    def handle_data(self, data):
        self.parts.append(data)


def fetch_json(url: str, user: str, token: str) -> dict:
    req = urllib.request.Request(url)
    auth_bytes = f"{user}:{token}".encode("utf-8")
    req.add_header("Authorization", "Basic " + base64.b64encode(auth_bytes).decode("utf-8"))
    req.add_header("Accept", "application/json")
    with urllib.request.urlopen(req) as resp:
        return json.load(resp)


def write_spec_and_meta(
    output_dir: str,
    content: dict,
    base_url: str,
) -> None:
    storage_html = content.get("body", {}).get("storage", {}).get("value", "")
    page_title = content.get("title", "Untitled")
    version_number = content.get("version", {}).get("number")
    links = content.get("_links", {})
    webui = links.get("webui")
    canonical_url = f"{base_url}{webui}" if webui else None

    parser = MinimalMarkdownParser()
    parser.feed(storage_html)
    text = "".join(parser.parts).strip()

    spec_path = os.path.join(output_dir, "spec.md")
    meta_path = os.path.join(output_dir, "meta.json")

    with open(spec_path, "w", encoding="utf-8") as f:
        f.write(f"# {page_title}\n\n")
        if canonical_url:
            f.write(f"Source: {canonical_url}\n\n")
        f.write(text + "\n")

    meta = {
        "page_id": content.get("id"),
        "fetched_at_utc": datetime.now(timezone.utc)
        .isoformat()
        .replace("+00:00", "Z"),
        "confluence_version": version_number,
    }
    if canonical_url:
        meta["canonical_url"] = canonical_url

    with open(meta_path, "w", encoding="utf-8") as f:
        json.dump(meta, f, indent=2)
        f.write("\n")


def iter_child_page_ids(base_url: str, user: str, token: str, root_page_id: str) -> list[str]:
    page_ids: list[str] = []
    start = 0
    limit = 50
    while True:
        url = (
            f"{base_url}/rest/api/content/{root_page_id}/child/page"
            f"?limit={limit}&start={start}"
        )
        data = fetch_json(url, user, token)
        results = data.get("results", [])
        page_ids.extend([item["id"] for item in results if "id" in item])
        if data.get("size", 0) + start >= data.get("totalSize", 0):
            break
        start += limit
    return page_ids


def mirror_architecture(base_url: str, user: str, token: str, root_page_id: str) -> None:
    base_dir = os.path.join("SpecsMirror", "Architecture")
    os.makedirs(base_dir, exist_ok=True)

    page_ids = iter_child_page_ids(base_url, user, token, root_page_id)
    for page_id in page_ids:
        url = (
            f"{base_url}/rest/api/content/{page_id}"
            "?expand=body.storage,version,_links,ancestors"
        )
        content = fetch_json(url, user, token)
        ancestors = content.get("ancestors", [])
        path_parts: list[str] = []
        root_index = None
        for idx, ancestor in enumerate(ancestors):
            if ancestor.get("id") == root_page_id:
                root_index = idx
                break
        if root_index is not None:
            for ancestor in ancestors[root_index + 1 :]:
                title = ancestor.get("title", "Untitled")
                path_parts.append(sanitize_path_part(title))
        title = content.get("title", "Untitled")
        path_parts.append(sanitize_path_part(title))

        output_dir = os.path.join(base_dir, *path_parts)
        os.makedirs(output_dir, exist_ok=True)
        write_spec_and_meta(output_dir, content, base_url)


def main() -> None:
    base_url = get_env("CONFLUENCE_BASE_URL").rstrip("/")
    user = get_env("CONFLUENCE_USER_EMAIL")
    token = get_env("CONFLUENCE_API_TOKEN")
    root_page_id = get_env("ARCH_ROOT_PAGE_ID")

    mirror_architecture(base_url, user, token, root_page_id)


if __name__ == "__main__":
    main()
