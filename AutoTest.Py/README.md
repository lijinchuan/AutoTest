# AutoDesk — 桌面程序自动化测试框架

基于 **pytest + uiautomation + allure** 的桌面应用自动化测试工具。

> Python = 现在的 JS 测试脚本。C# AutoTest 平台负责用例管理、调度、数据存储；Python 引擎负责桌面自动化执行。

## 系统要求

- **Python** >= 3.10（[python.org](https://www.python.org/downloads/) 下载安装）
- **Allure CLI**（报告生成，[安装指南](https://docs.qameta.io/allure-report/#_installing_a_commandline)）
- **Windows 10+**（uiautomation 依赖 Windows UI Automation API）

## 安装

```bash
cd AutoTest.Py
pip install -r requirements.txt
```

## 快速开始

### 1. 配置被测应用

编辑 `config/settings.yaml`：

```yaml
apps:
  notepad:
    name: "Notepad"
    executable: "notepad.exe"
    window_title: "无标题 - 记事本"  # Windows 11 中文版
    window_class: "Notepad"
    startup_timeout: 10
```

### 2. 运行测试

```bash
# 运行所有 Notepad 测试
pytest src/autodesk/tests/notepad/ -v --alluredir=results/allure

# 或使用 CLI
python -m src.cli run -m notepad
```

### 3. 查看报告

```bash
# 生成并打开 Allure HTML 报告
allure serve results/allure

# 或使用 CLI
python -m src.cli allure-serve
```

## 项目结构

```
AutoTest.Py/
├── config/
│   ├── settings.yaml       # 被测应用和框架配置
│   └── envs.yaml           # 环境变量
├── src/
│   ├── cli.py              # CLI 入口
│   └── autodesk/
│       ├── core/           # 核心模块（config, logger, exceptions）
│       ├── desktop/        # 桌面自动化层（app, page, element, locator）
│       ├── pom/            # Page Object Model（被测应用页面）
│       ├── tests/          # pytest 测试用例
│       ├── utils/          # 工具（wait, screenshot）
│       ├── report/         # Allure 报告辅助
│       └── runner.py       # C# ↔ Python 桥梁
├── conftest.py             # 全局 pytest fixtures
└── results/                # 测试结果输出（allure + screenshots）
```

## 架构概念对照

| C# 概念 | Python 映射 |
|---|---|
| `TestSource` | `@pytest.mark.notepad` |
| `TestPage` | Page Object 类 (`NotepadMainWindow`) |
| `TestCase` | `test_*` 函数 |
| `TestCode` (JS) | Page Object 方法调用 (Python) |
| `ValidCode` (JS) | `assert` 语句 |
| `TestResult` | pytest exit code + allure |
| `WebTask` | `Application.launch()` → 执行 → `close()` |

## 编写测试脚本

### Page Object

```python
from autodesk.desktop.page import BasePage
from autodesk.desktop.element import DesktopElement
from autodesk.desktop.locator import Locator

class NotepadMainWindow(BasePage):
    EDIT_AREA = Locator.by_control_type("EditControl")

    def type_text(self, text: str):
        DesktopElement(self._find_element(self.EDIT_AREA), self.EDIT_AREA).set_text(text)
        return self
```

### 测试用例

```python
import pytest
import allure

@pytest.mark.desktop
@pytest.mark.notepad
class TestNotepad:
    @allure.feature("Notepad")
    @allure.title("输入文本并验证")
    def test_type_text(self, notepad_main):
        notepad_main.type_text("Hello, AutoDesk!")
        assert "Hello, AutoDesk!" in notepad_main.get_text()
```

### fixture 生命周期

fixture 自动管理应用启动和关闭：

```python
@pytest.fixture
def notepad_app(global_config):
    app = Application(ConfigLoader.get_app("notepad"))
    app.launch()
    yield app       # 测试执行
    app.close()     # 自动清理
```

## C# 集成

在 `AutoTest.UI` 中，创建 `TestMode = "Desktop"` 的 `TestCase` 即可使用桌面测试：

```csharp
// C# 会自动调用 DesktopTestRunner
var testCase = new TestCase
{
    CaseName = "记事本测试",
    TestCode = "notepad_main.type_text('hello')",
    TestMode = "Desktop"
};
```

C# 侧通过 `DesktopTestRunner` 启动 Python 进程执行测试，解析 JSON 结果并存入 `TestResult`。
