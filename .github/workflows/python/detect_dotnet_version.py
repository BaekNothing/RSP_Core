import json
from pathlib import Path


def main() -> None:
    global_json = Path("global.json")
    if not global_json.exists():
        print("8.0.x")
        return

    data = json.loads(global_json.read_text(encoding="utf-8"))
    version = data.get("sdk", {}).get("version", "")
    if not version:
        raise SystemExit("global.json found but sdk.version missing")

    print(version)


if __name__ == "__main__":
    main()
