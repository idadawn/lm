import sys
import os

file_path = r"e:\project\2025\lm\web\src\views\lab\intermediateDataFormula\components\AdvancedJudgmentEditor.vue"

try:
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    # 1-based line 81 is index 80
    start_idx = 80 
    # 1-based line 312 is index 311. Slice end is exclusive, so we use 312.
    end_idx = 312 

    # Verification
    # Line 81 content check
    if "<!-- 原条件组卡片实现" not in lines[start_idx]:
        print(f"Error: Line {start_idx+1} does not match expected start. Got: {lines[start_idx].strip()}")
        sys.exit(1)

    # Line 312 content check
    # Note: Indentation matters for exact string match, but strip() handles it.
    if lines[end_idx-1].strip() != "</div>":
        print(f"Error: Line {end_idx} does not match expected end '</div>'. Got: {lines[end_idx-1].strip()}")
        # Let's print the surrounding lines for context if it fails
        print(f"Context: {lines[end_idx-2].strip()} -> {lines[end_idx-1].strip()} -> {lines[end_idx].strip()}")
        sys.exit(1)

    # Delete
    print(f"Deleting {end_idx - start_idx} lines...")
    del lines[start_idx:end_idx]

    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(lines)
    print("Successfully deleted lines.")

except Exception as e:
    print(f"An error occurred: {e}")
    sys.exit(1)
