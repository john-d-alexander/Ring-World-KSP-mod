"""Read one release from the consolidated, user-facing Markdown history."""
import re
from pathlib import Path


def release_body(path: Path, version: str) -> str:
    text = path.read_text(encoding="utf-8-sig")
    marker = "## Release-v" + version
    matches = list(re.finditer(r"^" + re.escape(marker) + r"[ \t]*$", text, re.MULTILINE))
    if len(matches) != 1:
        raise ValueError(f"Expected exactly one {marker} section in {path}")
    tail = text[matches[0].end():]
    boundary = re.search(r"^## (?:Release|Beta)-v", tail, re.MULTILINE)
    body = tail[:boundary.start()] if boundary else tail
    # Published descriptions are retained for history, not duplicated on republishing.
    body = body.split("### Published GitHub description", 1)[0]
    body = re.sub(r"^\[Published release\]\([^\n]+\)\s*", "", body.lstrip())
    if not body.strip():
        raise ValueError(f"No release notes for {version}")
    return body.strip() + "\n"
