import base64
import json
import os
import urllib.request
from datetime import datetime, timezone
from html.parser import HTMLParser


def get_env(name: str) -> str:
    value = os.environ.get(name, "")
    if not value:
        raise SystemExit(f"Missing required environment variable: {name}")
    return value


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


def main() -> None:
    base_url = get_env("CONFLUENCE_BASE_URL").rstrip("/")
    user = get_env("CONFLUENCE_USER_EMAIL")
    token = get_env("CONFLUENCE_API_TOKEN")
    feature_key = get_env("FEATURE_KEY")
    page_id = get_env("PAGE_ID")

    url = f"{base_url}/rest/api/content/{page_id}?expand=body.storage,version,_links"
    req = urllib.request.Request(url)
    auth_bytes = f"{user}:{token}".encode("utf-8")
    req.add_header("Authorization", "Basic " + base64.b64encode(auth_bytes).decode("utf-8"))
    req.add_header("Accept", "application/json")

    with urllib.request.urlopen(req) as resp:
        data = json.load(resp)

    storage_html = data.get("body", {}).get("storage", {}).get("value", "")
    page_title = data.get("title", "Untitled")
    version_number = data.get("version", {}).get("number")
    links = data.get("_links", {})
    webui = links.get("webui")
    canonical_url = f"{base_url}{webui}" if webui else None

    parser = MinimalMarkdownParser()
    parser.feed(storage_html)
    text = "".join(parser.parts).strip()

    spec_path = os.path.join("SpecsMirror", feature_key, "spec.md")
    meta_path = os.path.join("SpecsMirror", feature_key, "meta.json")

    with open(spec_path, "w", encoding="utf-8") as f:
        f.write(f"# {page_title}\n\n")
        if canonical_url:
            f.write(f"Source: {canonical_url}\n\n")
        f.write(text + "\n")

    meta = {
        "page_id": page_id,
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


if __name__ == "__main__":
    main()
