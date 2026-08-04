"""
诊断脚本：检查 Windows 计算器的 UI 控件树结构
"""
import subprocess
import time
import uiautomation as uia


def dump_children(ctrl, depth=0, max_depth=5, max_children=10):
    """递归打印子控件"""
    if depth > max_depth:
        return
    try:
        children = ctrl.GetChildren()
        for i, child in enumerate(children[:max_children]):
            indent = "  " * depth
            try:
                name = child.Name or ""
                cname = child.ClassName or ""
                aid = child.AutomationId or ""
                ctype = child.ControlTypeName or ""
            except Exception:
                name, cname, aid, ctype = "(stale)", "", "", ""
            print(f"{indent}[L{depth}] {ctype} "
                  f'Name="{name}" '
                  f'Class="{cname}" '
                  f'AutoId="{aid}"')
            # 继续深入
            dump_children(child, depth + 1, max_depth, min(max_children, 6))
        if len(children) > max_children:
            print(f"{'  ' * depth}  ... +{len(children) - max_children} more")
    except Exception as e:
        print(f"{'  ' * depth}  ERROR: {type(e).__name__}: {e}")


def main():
    print("Starting calc.exe ...")
    subprocess.Popen("calc.exe")
    time.sleep(2.0)

    # 查找计算器窗口
    desktop = uia.GetRootControl()
    found = None
    for top in desktop.GetChildren():
        try:
            n = top.Name or ""
        except Exception:
            continue
        # 匹配中文"计算器"或英文"Calculator"
        if "计算器" in n or "Calculator" in n or "alculator" in n:
            found = top
            print(f'Found window: Name="{n}" Class="{top.ClassName}"')
            break

    if not found:
        print("Calculator window not found! Listing all top-level windows:")
        for top in desktop.GetChildren():
            try:
                if top.Name:
                    print(f'  "{top.Name}" (Class={top.ClassName})')
            except Exception:
                pass
        return

    print("\n=== UI Tree (depth 5) ===")
    dump_children(found, max_depth=5, max_children=8)

    # 尝试单独枚举所有 Button
    print("\n=== Searching for digit/operator buttons (deep) ===")
    try:
        # 直接在窗口上搜所有 ButtonControl（深度搜索）
        buttons = []
        def collect_buttons(ctrl, depth=0, max_depth=10):
            if depth > max_depth:
                return
            try:
                for child in ctrl.GetChildren():
                    try:
                        ct = child.ControlTypeName
                    except Exception:
                        continue
                    if ct == "ButtonControl":
                        try:
                            buttons.append((child.Name, child.AutomationId))
                        except Exception:
                            pass
                    collect_buttons(child, depth + 1, max_depth)
            except Exception:
                pass
        collect_buttons(found)
        for name, aid in buttons[:30]:
            print(f'  Button: Name="{name}"  AutoId="{aid}"')
        print(f"  Total buttons: {len(buttons)}")
    except Exception as e:
        print(f"  Button search failed: {e}")

    # 关闭计算器
    print("\nClosing calculator via Alt+F4 ...")
    try:
        found.SetFocus()
        uia.SendKeys("{Alt}{F4}")
    except Exception as e:
        print(f"  Close failed: {e}")


if __name__ == "__main__":
    main()
