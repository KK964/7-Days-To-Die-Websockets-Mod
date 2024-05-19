#!/usr/bin/env sh

# Update local references to the latest version

if [ -z "$1" ]; then
  echo "Usage: $0 <path-to-game-directory>"
  exit 1
fi

GAME_DIR=$1

# Check if the game directory exists
if [ ! -d "$GAME_DIR" ]; then
  echo "Error: The game directory does not exist"
  exit 1
fi

# Expected files
#   GAME_DIR/7DaysToDie_Data/Managed/0Harmony.dll
#   GAME_DIR/7DaysToDie_Data/Managed/Assembly-CSharp.dll
#   GAME_DIR/7DaysToDie_Data/Managed/LogLibrary.dll
#   GAME_DIR/7DaysToDie_Data/Managed/UnityEngine.dll
#   GAME_DIR/7DaysToDie_Data/Managed/UnityEngine.CoreModule.dll

verify_file() {
  echo "Verifying $1"
  if [ ! -f "$1" ]; then
    echo "Error: $1 does not exist"
    exit 1
  fi
}

verify_file "$GAME_DIR/7DaysToDie_Data/Managed/0Harmony.dll"
verify_file "$GAME_DIR/7DaysToDie_Data/Managed/Assembly-CSharp.dll"
verify_file "$GAME_DIR/7DaysToDie_Data/Managed/LogLibrary.dll"
verify_file "$GAME_DIR/7DaysToDie_Data/Managed/UnityEngine.dll"
verify_file "$GAME_DIR/7DaysToDie_Data/Managed/UnityEngine.CoreModule.dll"

echo "All files exist"

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
LOCAL_REFS_DIR="$SCRIPT_DIR/../localrefs"

echo "Updating local references..."
echo "\tGame directory: $GAME_DIR"
echo "\tLocal references directory: $LOCAL_REFS_DIR"

# Ensure the local references directory exists
mkdir -p "$LOCAL_REFS_DIR"

copy_file() {
  echo "Copying $1 to $2"
  cp -f "$1" "$2"
  if [ $? -ne 0 ]; then
    echo "Error: Failed to copy $1 to $2"
    exit 1
  fi
}

copy_file "$GAME_DIR/7DaysToDie_Data/Managed/0Harmony.dll" "$LOCAL_REFS_DIR/0Harmony.dll"
copy_file "$GAME_DIR/7DaysToDie_Data/Managed/Assembly-CSharp.dll" "$LOCAL_REFS_DIR/Assembly-CSharp.dll"
copy_file "$GAME_DIR/7DaysToDie_Data/Managed/LogLibrary.dll" "$LOCAL_REFS_DIR/LogLibrary.dll"
copy_file "$GAME_DIR/7DaysToDie_Data/Managed/UnityEngine.dll" "$LOCAL_REFS_DIR/UnityEngine.dll"
copy_file "$GAME_DIR/7DaysToDie_Data/Managed/UnityEngine.CoreModule.dll" "$LOCAL_REFS_DIR/UnityEngine.CoreModule.dll"

echo "Local references updated successfully"

# Give some time to read the message
sleep 2
