"""
Notepad 基本功能测试

对应现有 C# 项目中的 TestCase 概念:
  每个 test_* 方法 ≈ 一个 TestCase 实例
  TestCode ≈ Page Object 方法调用
  ValidCode ≈ assert 断言

运行方式:
  pytest src/autodesk/tests/notepad/test_notepad_basic.py -v --alluredir=results/allure
"""
import time

import allure
import pytest


@pytest.mark.desktop
@pytest.mark.notepad
class TestNotepadBasic:
    """
    Notepad 基本文本操作测试。
    对应 C# 中一组 TestCase 的集合。
    """

    @allure.feature("Notepad")
    @allure.story("文本输入")
    @allure.title("输入文本并验证内容出现")
    @allure.description("在记事本中输入文本，验证内容正确显示")
    def test_type_text(self, notepad_main):
        """测试基本文本输入 — 对应单个 TestCase"""
        notepad_main.type_text("Hello, AutoDesk!")
        time.sleep(0.3)
        text = notepad_main.get_text()
        assert "Hello, AutoDesk!" in text, f"文本内容不匹配: {text}"

    @allure.feature("Notepad")
    @allure.story("文本输入")
    @allure.title("中文文本输入")
    @allure.description("验证记事本支持中文输入")
    def test_type_chinese_text(self, notepad_main):
        """测试中文输入"""
        notepad_main.type_text("你好，自动化测试！")
        time.sleep(0.3)
        text = notepad_main.get_text()
        assert "你好，自动化测试！" in text, f"中文内容不匹配: {text}"

    @allure.feature("Notepad")
    @allure.story("文本编辑")
    @allure.title("清空所有文本")
    @allure.description("验证全选删除功能")
    def test_clear_text(self, notepad_main):
        """测试清空文本"""
        notepad_main.type_text("Some sample text to be cleared")
        time.sleep(0.3)
        notepad_main.clear_text()
        time.sleep(0.3)
        text = notepad_main.get_text().strip()
        assert text == "", f"清空后仍有文本: '{text}'"

    @allure.feature("Notepad")
    @allure.story("窗口管理")
    @allure.title("验证窗口标题")
    @allure.description("确认记事本窗口标题正确")
    def test_window_title(self, notepad_main):
        """测试窗口标题"""
        title = notepad_main.get_title()
        assert "记事本" in title or "Notepad" in title, f"窗口标题异常: {title}"


@pytest.mark.desktop
@pytest.mark.notepad
class TestNotepadEditFlow:
    """
    Notepad 编辑工作流测试。
    演示多步骤的端到端测试。
    """

    @allure.feature("Notepad")
    @allure.story("编辑流程")
    @allure.title("输入 → 清空 → 重新输入 → 验证")
    @allure.description("模拟用户编辑工作流")
    def test_edit_workflow(self, notepad_main):
        """端到端编辑流程"""
        # Step 1: 输入
        with allure.step("输入第一段文本"):
            notepad_main.type_text("First paragraph")
            time.sleep(0.2)

        # Step 2: 验证
        with allure.step("验证第一段文本"):
            assert "First paragraph" in notepad_main.get_text()

        # Step 3: 替换文本
        with allure.step("替换文本"):
            notepad_main.replace_text("Second paragraph")
            time.sleep(0.2)

        # Step 4: 最终验证
        with allure.step("最终验证"):
            text = notepad_main.get_text()
            assert "Second paragraph" in text
            assert "First paragraph" not in text, "旧文本应当已被替换"

    @allure.feature("Notepad")
    @allure.story("编辑流程")
    @allure.title("快速连续输入")
    @allure.description("验证快速连续输入不会丢字")
    def test_rapid_typing(self, notepad_main):
        """快速连续输入 — 对应现有 WebTask 中的重试逻辑场景"""
        words = ["Hello", "World", "AutoDesk", "Desktop", "Testing"]
        for word in words:
            notepad_main.type_text(word + " ")
            time.sleep(0.1)

        text = notepad_main.get_text()
        for word in words:
            assert word in text, f"缺失: {word}"
