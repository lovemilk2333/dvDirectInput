#!/bin/bash
set -e

NO_ARCHIVE=false
OUTPUT_DIRECTORY="$(cd "$(dirname "$0")" && pwd)"

while [[ $# -gt 0 ]]; do
    case $1 in
        --no-archive) NO_ARCHIVE=true; shift ;;
        --output-directory) OUTPUT_DIRECTORY="$2"; shift 2 ;;
        *) echo "Unknown param: $1"; exit 1 ;;
    esac
done

cd "$(dirname "$0")"
SCRIPT_DIR="$(pwd)"

# CSPROJ=$(*.csproj)
CSPROJ="dvDirectInput/dvDirectInput.csproj"
VERSION=$(grep -oP '<Version>\K[0-9]+\.[0-9]+\.[0-9]+' "$CSPROJ" | head -1)
MOD_NAME=$(grep -oP '<AssemblyName>\K[^<]+' "$CSPROJ" | head -1)

echo "Packaging $MOD_NAME version $VERSION"

DIST_DIR="$OUTPUT_DIRECTORY/dist"
if [ "$NO_ARCHIVE" = true ]; then
    ZIP_WORK_DIR="$OUTPUT_DIRECTORY"
else
    ZIP_WORK_DIR="$DIST_DIR/tmp"
fi

ZIP_ROOT_DIR="$ZIP_WORK_DIR/$MOD_NAME"
LICENSE_FILE="LICENSE"
ASSEMBLY_FILES="build/"

mkdir -p "$ZIP_ROOT_DIR"
cp -f "$LICENSE_FILE" "$ASSEMBLY_FILES"* "$ZIP_ROOT_DIR" 2>/dev/null || true

cat > "${ZIP_ROOT_DIR}/info.json" << EOF
{
	"Id": "${MOD_NAME}",
	"DisplayName": "${MOD_NAME}",
	"Author": "miruku (lovemilk)",
	"Version": "${VERSION}",
	"AssemblyName": "${MOD_NAME}.dll",
	"EntryMethod": "${MOD_NAME}.Main.Load",
	"ManagerVersion": "0.27.3",
	"Repository": "https://raw.githubusercontent.com/lovemilk2333/dvDirectInput/main/repostiory.json"
}
EOF

if [ "$NO_ARCHIVE" = false ]; then
    ARCHIVE_PATH="${DIST_DIR}/${MOD_NAME}_v${VERSION}.zip"
    mkdir -p "$DIST_DIR"
    cd "$ZIP_WORK_DIR"
    zip -r "$ARCHIVE_PATH" "$MOD_NAME"
    cd "$SCRIPT_DIR"
    rm -rf "$ZIP_WORK_DIR"
    echo "Archived at $ARCHIVE_PATH"
else
    echo "Copied Assemblies, License and created Info file to ${ZIP_ROOT_DIR}"
fi

cd "$SCRIPT_DIR"

REPOSITORY_PATH="$SCRIPT_DIR/repository.json"
if [ -f "$REPOSITORY_PATH" ]; then
    LATEST_VERSION=$(grep -oP '"Version": "\K[0-9]+\.[0-9]+\.[0-9]+' "$REPOSITORY_PATH" | head -1)
    if [ "$LATEST_VERSION" != "$VERSION" ]; then
        echo "New version detected ($VERSION is not $LATEST_VERSION). Updating $REPOSITORY_PATH"
        DOWNLOAD_URL="https://github.com/lovemilk2333/dvDirectInput/releases/download/v${VERSION}/${MOD_NAME}_v${VERSION}.zip"
        export REPO_PATH="$REPOSITORY_PATH"
        export M_NAME="$MOD_NAME"
        export M_VER="$VERSION"
        export D_URL="$DOWNLOAD_URL"

python3 -c '
import json, os, sys
path = os.getenv("REPO_PATH")
try:
    with open(path, "r") as f:
        data = json.load(f)
except FileNotFoundError:
    data = {}

new_entry = {
    "Id": os.getenv("M_NAME"),
    "Version": os.getenv("M_VER"),
    "DownloadUrl": os.getenv("D_URL")
}

data.setdefault("Releases", []).insert(0, new_entry)

with open(path, "w") as f:
    json.dump(data, f, indent=4)
    f.write("\n")
print(f"Updated {path}")
'
    fi
fi
