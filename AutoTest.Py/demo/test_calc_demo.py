"""
计算器加法测试 - 正确版本
使用 AutomationId 定位元素（跨语言通用，不依赖中文/英文 UI）
"""
import subprocess
import time
import uiautomation as uia


def test_addition():
    """测试 1 + 2 = 3"""
    # 1. 启动计算器
    print("启动计算器...")
    subprocess.Popen("calc.exe")
    time.sleep(1.5)

    # 2. 获取计算器窗口
    # UWP 应用外层是 ApplicationFrameWindow，内层 CoreWindow 才是实际内容
    frame = uia.WindowControl(searchDepth=1, ClassName="ApplicationFrameWindow")
    if not frame.Exists(0, 0.5):
        raise Exception("未找到计算器窗口 (ApplicationFrameWindow)")

    # 通过 AutomationId 定位结果展示区（验证窗口正确）
    result = frame.TextControl(AutomationId="CalculatorResults")
    if not result.Exists(3, 0.5):
        raise Exception("未找到结果显示控件 (CalculatorResults)")
    print(f"计算器已就绪，当前显示: {result.Name}")

    # 3. 执行 1 + 2 =
    # ★ 用 AutomationId 找按钮，不依赖中文 Name
    frame.ButtonControl(AutomationId="num1Button").Click()
    time.sleep(0.15)
    frame.ButtonControl(AutomationId="plusButton").Click()
    time.sleep(0.15)
    frame.ButtonControl(AutomationId="num2Button").Click()
    time.sleep(0.15)
    frame.ButtonControl(AutomationId="equalButton").Click()
    time.sleep(0.3)

    # 4. 验证结果
    # 刷新 result 引用（点击后可能变 stale）
    result = frame.TextControl(AutomationId="CalculatorResults")
    result_text = result.Name if result.Exists(0, 0.3) else ""
    print(f"计算结果: {result_text}")

    # "显示为 3" → 提取数字部分
    assert "3" in result_text.replace("显示为 ", "").replace(" ", ""), \
        f"期望结果为 3，实际: {result_text}"

    # 5. 关闭计算器
    print("关闭计算器...")
    frame.SendKeys("{Alt}{F4}")
    print("测试通过!")


# ============================================================
# 如果要在 C# 框架的 TestCode 中运行（不需要自己写 subprocess 和
# 窗口查找），用下面这个版本：
# ============================================================
def test_addition_simple():
    """
    简化版：适合已经通过框架启动了计算器的情况
    直接假设计算器窗口已打开，直接用 AutomationId 操作
    """
    import uiautomation as uia
    import time

    # ★ 关键：UWP 应用用 ClassName 定位，而不是 Name
    calc = uia.WindowControl(searchDepth=1, ClassName="ApplicationFrameWindow")

    if not calc.Exists(3, 0.5):
        raise Exception("计算器窗口未找到，请先启动 calc.exe")

    # 点击 1 + 2 =
    calc.ButtonControl(AutomationId="num1Button").Click()
    time.sleep(0.2)
    calc.ButtonControl(AutomationId="plusButton").Click()
    time.sleep(0.2)
    calc.ButtonControl(AutomationId="num2Button").Click()
    time.sleep(0.2)
    calc.ButtonControl(AutomationId="equalButton").Click()
    time.sleep(0.3)

    # 读结果
    result = calc.TextControl(AutomationId="CalculatorResults")
    text = result.Name
    print(f"结果: {text}")
    assert "3" in text, f"期望 3，实际: {text}"


if __name__ == "__main__":
    test_addition()
