﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using LJC.FrameWorkV3.CodeExpression.KeyWordMatch;
using LJC.FrameWorkV3.Comm;
using AutoTest.Domain.Entity;
using AutoTest.Util;

namespace AutoTest.UI.UC
{
    public partial class EditTextBox : UserControl
    {
        public class ThinkInfo
        {
            /// <summary>
            /// 0-关键字 1-表 2-字段
            /// </summary>
            public int Type
            {
                get;
                set;
            }

            /// <summary>
            /// 对象名
            /// </summary>
            public string ObjectName
            {
                get;
                set;
            }

            /// <summary>
            /// 关联内容
            /// </summary>
            public object Tag
            {
                get;
                set;
            }

            /// <summary>
            /// 描述
            /// </summary>
            public string Desc
            {
                get;
                set;
            }

            /// <summary>
            /// 匹配打分
            /// </summary>
            public byte Score
            {
                get;
                set;
            }
        }

        public class ViewContext
        {
            /// <summary>
            ///0-空 1-字段提示 2-联想
            /// </summary>
            public byte DataType
            {
                get;
                set;
            }
        }

        private Color defaultSelectionColor;
        //internal KeyWordManager keywordman = new KeyWordManager();
        private KeyWordManager _keyWords = new KeyWordManager();
        private WatchTimer _timer = new WatchTimer(3);
        private System.Threading.Timer backtimer = null;
        private List<int> _markedLines = new List<int>();
        private int _lastMarketedLines = -1;
        private int _lastInputChar = '\0';
        private Point _currpt = Point.Empty;
        private DateTime _pointtiptime = DateTime.MaxValue;

        private DataGridView view = new DataGridView();

        /// <summary>
        /// 备选库
        /// </summary>
        private List<ThinkInfo> ThinkInfoLib = null;
        private HashSet<string> TableSet = new HashSet<string>();
        private object GetObjects(string keys, ref int count)
        {
            if (ThinkInfoLib == null)
            {
                ThinkInfoLib = new List<ThinkInfo>();

                foreach (var o in ScriptKeyWordHelper.GetKeyWordList())
                {
                    ThinkInfoLib.Add(new ThinkInfo
                    {
                        Type = 0,
                        Desc = o.Desc,
                        ObjectName = o.KeyWord
                    });
                }
            }

            var searchtable = string.Empty;
            if (keys.IndexOf('.') > -1)
            {
                var keyarr = keys.Split('.');
                searchtable = keyarr[keyarr.Length - 2];
                keys = keyarr.Last();
            }

            List<ThinkInfo> thinkresut = new List<ThinkInfo>();
            var sourceList = ThinkInfoLib.Select(p => new SubSearchSourceItem(p.ObjectName.ToUpper(), p)).ToList();
            sourceList.AddRange(ThinkInfoLib.Where(p => !string.IsNullOrWhiteSpace(p.Desc)).Select(p => new SubSearchSourceItem(p.Desc.ToUpper(), p)).ToList());
            thinkresut = StringHelper.SubSearch(sourceList, keys.ToUpper(), 3, 1000)
                .Select(p => (ThinkInfo)p).ToList();

            foreach (var item in TableSet.Select(p => p).ToList())
            {
                if (this.RichText.Text.IndexOf(item, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    TableSet.Remove(item);
                }
            }

            thinkresut = thinkresut.OrderByDescending(p => p.Score).ThenBy(p => p.ObjectName.Length).Take(250).ToList();

            count = thinkresut.Count;
            return thinkresut.Select(p =>
            {
                string objectname = null;
                string replaceobjectname = null;
                bool issamedb = true;

                objectname = p.ObjectName;
                replaceobjectname = p.ObjectName;
                return new
                {
                    建议 = objectname,
                    说明 = p.Desc,
                    p.Type,
                    Issamedb = issamedb,
                    replaceobjectname
                };
            }).ToList();
        }

        public KeyWordManager KeyWords
        {
            get
            {
                return _keyWords;
            }
        }

        public override string Text
        {
            get
            {
                return RichText.Text;
            }
            set
            {
                this.RichText.Text = value;
                this.SetLineNo();
            }
        }

        public int SelectionLength
        {
            get
            {
                return this.RichText.SelectionLength;
            }
        }

        public string SelectedText
        {
            get
            {
                return this.RichText.SelectedText;
            }
            set
            {
                this.RichText.Text = value;
            }
        }

        public void Init(IEnumerable<ScriptKeyWord> scriptKeyWords)
        {
            if (ThinkInfoLib == null)
            {
                ThinkInfoLib = new List<ThinkInfo>();

                foreach (var o in ScriptKeyWordHelper.GetKeyWordList())
                {
                    ThinkInfoLib.Add(new ThinkInfo
                    {
                        Type = 0,
                        Desc = o.Desc,
                        ObjectName = o.KeyWord
                    });
                }
            }

            if (scriptKeyWords != null)
            {
                foreach(var item in scriptKeyWords)
                {
                    ThinkInfoLib.Add(new ThinkInfo
                    {
                        Desc=item.Desc,
                        ObjectName=item.KeyWord,
                        Type=0
                    });
                    KeyWords.AddKeyWord(item.KeyWord, item.HighColor);
                }
            }
        }

        public EditTextBox()
        {
            InitializeComponent();
            this.RichText.WordWrap = false;
            this.DoubleBuffered = true;
            this.RichText.ScrollBars = RichTextBoxScrollBars.Both;
            this.RichText.ContextMenuStrip = this.contextMenuStrip1;
            this.RichText.VScroll += new EventHandler(RichText_VScroll);
            this.ScaleNos.Font = new Font(RichText.Font.FontFamily, RichText.Font.Size + 1.019f);
            this.RichText.KeyUp += new KeyEventHandler(RichText_KeyUp);
            this.RichText.KeyDown += RichText_KeyDown;
            this.RichText.TextChanged += new EventHandler(RichText_TextChanged);
            this.RichText.MouseClick += RichText_MouseClick;
            this.RichText.MouseMove += RichText_MouseMove;
            this.RichText.MouseLeave += RichText_MouseLeave;
            this.RichText.DoubleClick += RichText_DoubleClick;
            contextMenuStrip1.VisibleChanged += ContextMenuStrip1_VisibleChanged;
            this.view.Font = new Font(RichText.Font.FontFamily, RichText.Font.Size + 1.019f);

            defaultSelectionColor = this.RichText.SelectionColor;

            view.Visible = false;
            view.MouseLeave += View_MouseLeave;
            view.BorderStyle = BorderStyle.None;
            view.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //view.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            view.AllowUserToAddRows = false;
            view.RowHeadersVisible = false;
            view.KeyUp += View_KeyUp;
            view.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            view.BackgroundColor = Color.White;
            view.GridColor = Color.LightGreen;
            view.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            view.CellClick += View_CellClick;
            view.RowPostPaint += View_RowPostPaint;
            view.DataBindingComplete += View_DataBindingComplete;

            剪切ToolStripMenuItem.Enabled = false;

            view.Tag = new ViewContext
            {
                DataType = 0
            };

            this.ParentChanged += EditTextBox_ParentChanged;

            this.RichText.ImeMode = ImeMode.On;

            this.RichText.HideSelection = false;

            backtimer = new System.Threading.Timer(new System.Threading.TimerCallback((o) =>
            {
                if (this.Visible)
                {
                    if (RichText.AutoWordSelection)
                        RichText.AutoWordSelection = false;
                }
                if (this.Visible && !view.Visible && _currpt != Point.Empty && DateTime.Now.Subtract(_pointtiptime).TotalMilliseconds >= 1000)
                {
                    _pointtiptime = DateTime.MaxValue;
                    backtimer.Change(0, Timeout.Infinite);
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            ShowTip();
                        }
                        catch (Exception ex)
                        {
                        }
                        finally
                        {
                            backtimer.Change(0, 100);
                        }
                    }));
                }
            }), null, 0, 100);

            foreach (var x in ScriptKeyWordHelper.GetKeyWordList())
            {
                this.KeyWords.AddKeyWord(x.KeyWord, x.HighColor);
            }
        }

        private void ContextMenuStrip1_VisibleChanged(object sender, EventArgs e)
        {
            this.重做ToolStripMenuItem.Enabled = this.RichText.CanRedo;
            this.撤消ToolStripMenuItem.Enabled = this.RichText.CanUndo;
        }

        private void RichText_DoubleClick(object sender, EventArgs e)
        {
            int st;
            var seltext = GetTipCurrWord(false, out st);
            if (string.IsNullOrWhiteSpace(seltext) || seltext.IndexOf('\n') > -1)
            {
                return;
            }
            this.RichText.Select(st - seltext.Length, seltext.Length);
        }

        private void ShowTip()
        {

        }

        private void RichText_MouseLeave(object sender, EventArgs e)
        {
            this._pointtiptime = DateTime.MaxValue;
            this._currpt = Point.Empty;
        }

        private void RichText_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currpt != e.Location)
            {
                _currpt = e.Location;
                _pointtiptime = DateTime.Now;
            }
        }

        private void RichText_MouseClick(object sender, MouseEventArgs e)
        {
            if (view.Visible)
            {
                var tippt = this.RichText.GetPositionFromCharIndex(this.RichText.SelectionIndent);
                if (Math.Abs(e.X - tippt.X) > 10 && Math.Abs(e.Y - tippt.Y) > 10)
                {
                    view.Visible = false;
                }
            }
        }

        private void View_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {

            if ((view.Tag as ViewContext).DataType == 2)
            {
                view.Columns["Type"].Visible = false;
                view.Columns["Issamedb"].Visible = false;
                view.Columns["replaceobjectname"].Visible = false;
            }

            var ajustviewwith = 0;
            int icount = 0;
            List<int> maxwidthlist = new List<int>();
            foreach (DataGridViewColumn col in view.Columns)
            {
                if (!col.Visible)
                {
                    continue;
                }

                int maxwidth = 0;
                foreach (DataGridViewRow row in view.Rows)
                {
                    using (var g = view.CreateGraphics())
                    {
                        var mwidth = col.DefaultCellStyle.Padding.Left + (int)g.MeasureString(row.Cells[col.Name].Value?.ToString() + col.Name, view.Font).Width + 20;
                        if (mwidth > maxwidth)
                        {
                            maxwidth = mwidth;
                        }
                    }
                }
                ajustviewwith += maxwidth;
                if (icount < view.DisplayedColumnCount(false))
                {
                    maxwidthlist.Add(maxwidth);
                }
                icount++;
            }

            var limitwidth = (int)(view.Parent?.Width ?? 800 * 0.7);
            var width = Math.Min(ajustviewwith, limitwidth);

            view.Width = width;

            var rate = width < ajustviewwith ? ((width * 1.0 / ajustviewwith)) : 1.0;
            icount = 0;
            foreach (DataGridViewColumn col in view.Columns)
            {
                if (!col.Visible)
                {
                    continue;
                }
                col.Width = (int)(maxwidthlist[icount] * rate);
                icount++;
            }

        }

        private void View_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if ((view.Tag as ViewContext).DataType == 2)
            {
                Bitmap rowIcon = null;

                var tp = (int)view.Rows[e.RowIndex].Cells["Type"].Value;
                if (tp == 1)
                {

                }
                else if (tp == 2)
                {

                }

                if (rowIcon != null)
                    e.Graphics.DrawImage(rowIcon, e.RowBounds.Left + 4, Convert.ToInt16((e.RowBounds.Top + e.RowBounds.Bottom) / 2 - 8), 16, 16);
            }

        }

        private void RichText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyData == (Keys.Control | Keys.Z))
            {
                e.Handled = true;
                RichText.Undo();
                return;
            }

            if (e.Control && e.KeyData == (Keys.Control | Keys.R))
            {
                e.Handled = true;
                RichText.Redo();
                return;
            }

            if (e.KeyCode == Keys.Down
                || e.KeyCode == Keys.Up)
            {
                if (view.Visible)
                {
                    View_KeyUp(this, new KeyEventArgs(e.KeyCode));
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Enter
                || e.KeyCode == Keys.Space
                || e.KeyCode == Keys.Right)
            {
                if (view.Visible)
                {
                    int i = 0;
                    for (; i < view.Rows.Count; i++)
                    {
                        if (view.Rows[i].Selected)
                        {
                            view.Rows[i].Selected = false;
                            break;
                        }
                    }
                    if (i < view.Rows.Count)
                    {
                        View_CellClick(e.KeyCode, new DataGridViewCellEventArgs(0, i));
                    }
                    else
                    {
                        view.Visible = false;
                    }
                    e.Handled = true;

                }
            }
        }

        private string GetTipCurrWord(bool includedot, out int start)
        {
            start = -1;
            var curindex = this.RichText.GetCharIndexFromPosition(_currpt);
            var realpt = this.RichText.GetPositionFromCharIndex(curindex);
            if (_currpt.X - realpt.X < -10 || _currpt.X - realpt.X > 15)
            {
                return string.Empty;
            }
            if (_currpt.Y - realpt.Y < -10 || _currpt.Y - realpt.Y > 15)
            {
                return string.Empty;
            }
            var currline = this.RichText.GetLineFromCharIndex(curindex);

            var charstartindex = this.RichText.GetFirstCharIndexFromLine(currline);
            var tippt = this.RichText.GetPositionFromCharIndex(curindex);
            tippt.Offset(0, 20);
            string pre = "", last = "";
            int pi = curindex - charstartindex - 1;

            //判断是否是注释部分
            var nodeindex = this.RichText.Lines[currline]?.IndexOf("//");
            if (nodeindex > -1 && pi >= nodeindex)
            {
                start = -1;
                return string.Empty;
            }

            while (pi >= 0)
            {

                var ch = this.RichText.Lines[currline][pi];

                if ((ch >= 'A' && ch <= 'Z') || (ch >= 48 && ch <= 57) || (ch >= 'a' && ch <= 'z')
                    || ch == '_' || ch == '@' || (includedot && ch == '.')
                    || (ch >= '\u4E00' && ch <= '\u9FA5'))
                {
                    pre = ch + pre;
                    pi--;
                }
                else
                {
                    break;
                }
            }
            pi = curindex - charstartindex;
            if (this.RichText.Lines.Length > currline)
            {
                while (pi < this.RichText.Lines[currline].Length)
                {
                    var ch = this.RichText.Lines[currline][pi];

                    if ((ch >= 'A' && ch <= 'Z') || (ch >= 48 && ch <= 57) || (ch >= 'a' && ch <= 'z')
                        || ch == '_' || ch == '@' || (includedot && ch == '.')
                        || (ch >= '\u4E00' && ch <= '\u9FA5'))
                    {
                        last += ch;
                        pi++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var keyword = pre + last;
            start = pi + charstartindex;
            return keyword;
        }

        private int GetCurrWord(out string word)
        {
            if (this.RichText.Lines.Length == 0)
            {
                word = string.Empty;
                return -1;
            }
            var curindex = this.RichText.SelectionStart;
            var currline = this.RichText.GetLineFromCharIndex(curindex);
            //var charstartindex = this.RichText.GetFirstCharIndexOfCurrentLine();
            var charstartindex = this.RichText.GetFirstCharIndexFromLine(currline);
            var tippt = this.RichText.GetPositionFromCharIndex(curindex);
            tippt.Offset(0, 20);
            string pre = "", last = "";
            int pi = curindex - charstartindex - 1;

            if (RichText.Lines.Length <= currline || pi >= RichText.Lines[currline].Length)
            {
                word = string.Empty;
                return -1;
            }

            //判断是否是注释部分
            var nodeindex = this.RichText.Lines[currline]?.IndexOf("//");
            if (nodeindex > -1 && pi >= nodeindex)
            {
                word = string.Empty;
                return -1;
            }

            while (pi >= 0)
            {

                var ch = this.RichText.Lines[currline][pi];

                if ((ch >= 'A' && ch <= 'Z') || (ch >= 48 && ch <= 57) || (ch >= 'a' && ch <= 'z')
                    || ch == '_' || ch == '.' || ch == '$'
                    || (ch >= '\u4E00' && ch <= '\u9FA5'))
                {
                    pre = ch + pre;
                    pi--;
                }
                else
                {
                    break;
                }
            }
            pi = curindex - charstartindex;
            if (this.RichText.Lines.Length > currline)
            {
                while (pi < this.RichText.Lines[currline].Length)
                {
                    var ch = this.RichText.Lines[currline][pi];

                    if ((ch >= 'A' && ch <= 'Z') || (ch >= 48 && ch <= 57) || (ch >= 'a' && ch <= 'z')
                        || ch == '_' || ch == '.' || ch == '$'
                        || (ch >= '\u4E00' && ch <= '\u9FA5'))
                    {
                        last += ch;
                        pi++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var keyword = pre + last;
            word = keyword;
            return pi + charstartindex;
        }

        private void View_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            if ((view.Tag as ViewContext).DataType == 2)
            {
                var val = view.Rows[e.RowIndex].Cells["replaceobjectname"].Value.ToString();
                var Issamedb = (bool)view.Rows[e.RowIndex].Cells["Issamedb"].Value;
                string keyword;
                var keywordindex = GetCurrWord(out keyword);
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    this.RichText.LockPaint = true;
                    //this.RichText.Select(this.RichText.SelectionStart - keyword.Length, keyword.Length);
                    this.RichText.Select(keywordindex - keyword.Length, keyword.Length);
                    //this.RichText.Text.Remove(this.RichText.SelectionStart - keyword.Length, keyword.Length);

                    if (val.StartsWith(this.RichText.SelectedText, StringComparison.OrdinalIgnoreCase))
                    {
                        this.RichText.SelectedText = val;
                    }
                    else
                    {
                        var lastText = RichText.SelectedText.Substring(RichText.SelectedText.LastIndexOf(".") + 1);

                        var subVal = val.Split('.').ToArray();
                        var replaceVal = string.Empty;
                        for (var i = subVal.Length - 1; i >= 0; i--)
                        {
                            if (subVal[i].StartsWith(lastText, StringComparison.OrdinalIgnoreCase))
                            {
                                replaceVal = string.Join(".", subVal.Skip(i));
                                break;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(replaceVal))
                        {
                            for (var i = subVal.Length - 1; i >= 0; i--)
                            {
                                if (subVal[i].IndexOf(lastText, StringComparison.OrdinalIgnoreCase) > -1)
                                {
                                    replaceVal = string.Join(".", subVal.Skip(i));
                                    break;
                                }
                            }
                        }


                        if (!string.IsNullOrWhiteSpace(replaceVal))
                        {
                            if (RichText.SelectedText.LastIndexOf(".") >= 0)
                            {
                                RichText.SelectedText = RichText.SelectedText.Remove(RichText.SelectedText.LastIndexOf(".")) + "." + replaceVal;
                            }
                            else
                            {
                                RichText.SelectedText = val;
                            }
                        }
                    }
                    
                    //this.RichText.SelectionStart += val.Length - keyword.Length;
                    view.Visible = false;
                    this.RichText.LockPaint = false;
                    this.RichText.Focus();
                }
            }
        }

        private void View_KeyUp(object sender, KeyEventArgs e)
        {
            int i = 0;
            for (; i < view.Rows.Count; i++)
            {
                if (view.Rows[i].Selected)
                {
                    view.Rows[i].Selected = false;
                    break;
                }
            }
            if (i == view.Rows.Count)
            {
                i = -1;
            }
            if (e.KeyCode == Keys.Up)
            {
                if (i < 0)
                {
                    view.Rows[view.Rows.Count - 1].Selected = true;
                    view.CurrentCell = view.Rows[view.Rows.Count - 1].Cells[0];
                }
                else if (i == 0)
                {
                    view.ClearSelection();
                }
                else
                {
                    view.Rows[i - 1].Selected = true;
                    view.CurrentCell = view.Rows[i - 1].Cells[0];
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (i == view.Rows.Count - 1)
                {
                    view.Rows[0].Selected = true;
                    view.CurrentCell = view.Rows[0].Cells[0];
                }
                else
                {
                    view.Rows[i + 1].Selected = true;
                    view.CurrentCell = view.Rows[i + 1].Cells[0];
                }
            }
        }

        private void EditTextBox_ParentChanged(object sender, EventArgs e)
        {
            if (this.Parent != null && !this.Parent.Controls.Contains(view))
            {
                this.Parent.Controls.Add(view);
            }
        }

        private void View_MouseLeave(object sender, EventArgs e)
        {
            if ((view.Tag as ViewContext).DataType == 2)
            {
                return;
            }
            view.Visible = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _lastMarketedLines = -1;
            MarkKeyWords(true);
        }

        void RichText_KeyUp(object sender, KeyEventArgs e)
        {
            if (!this.RichText.Focus())
                return;

            if (e.Control)
                return;
            if (e.Alt)
                return;
            //if (e.Shift)
            //    return;
            if (e.KeyData == (Keys.LButton | Keys.ShiftKey))
            {
                return;
            }
            _lastInputChar = e.KeyValue;
        }

        void RichText_TextChanged(object sender, EventArgs e)
        {
            if (_lastInputChar == '\0')
                return;
            if (this.RichText.Lines.Length == 0)
            {
                return;
            }

            #region 联想
            string keyword;
            var keywordindex = GetCurrWord(out keyword);

            if (!string.IsNullOrEmpty(keyword))
            {
                int count = 0;
                var obj = GetObjects(keyword, ref count);
                if (obj != null && count > 0)
                {
                    (view.Tag as ViewContext).DataType = 2;
                    view.DataSource = obj;

                    var padding = view.Columns[0].DefaultCellStyle.Padding;
                    padding.Left = 20;
                    view.Columns[0].DefaultCellStyle.Padding = padding;
                    view.Visible = true;

                    view.ClearSelection();
                    view.BringToFront();
                    view.Height = (view.Rows.GetRowsHeight(DataGridViewElementStates.Visible) / count) * Math.Min(10, count) + view.ColumnHeadersHeight;
                    //view.Height = ((view.Height- view.ColumnHeadersHeight)/count)*5+view.ColumnHeadersHeight;

                    var curindex = this.RichText.SelectionStart;
                    var tippt = this.RichText.GetPositionFromCharIndex(curindex);
                    tippt.Offset(RichText.Location.X + this.Location.X, 20 + this.Location.Y);
                    var morewidth = tippt.X + view.Width - view.Parent.Location.X - view.Parent.Width;
                    if (morewidth > 0)
                    {
                        tippt.Offset(-morewidth, 0);
                    }
                    if (view.Height + tippt.Y + 30 > Screen.GetWorkingArea(this).Height - this.Parent.PointToScreen(this.Parent.Location).Y)
                    {
                        tippt.Offset(0, -view.Height - 20);
                    }
                    view.ScrollBars = ScrollBars.Vertical;
                    view.Location = tippt;
                    view.BringToFront();
                }
                else
                {
                    (view.Tag as ViewContext).DataType = 0;
                    view.DataSource = null;
                    view.Visible = false;
                }
            }
            else
            {
                (view.Tag as ViewContext).DataType = 0;
                view.DataSource = null;
                view.Visible = false;
            }

            #endregion

            int line = this.RichText.GetLineFromCharIndex(this.RichText.GetFirstCharIndexOfCurrentLine());
            if (line >= RichText.Lines.Length)
            {
                return;
            }
            if (_lastInputChar == '\r' || _lastInputChar == '\n')
            {
                _markedLines.RemoveAll(p => p == line || p == line - 1);
                SetLineNo(true);
            }
            else if (_lastInputChar == '\b')
            {
                _markedLines.RemoveAll(p => p == line);
                SetLineNo();
            }
            else
            {
                _markedLines.RemoveAll(p => p == line);
            }
            _lastMarketedLines = -1;
            _lastInputChar = '\0';

            this.RichText.LockPaint = true;
            var oldstart = this.RichText.SelectionStart;
            var oldlen = this.RichText.SelectionLength;
            this.RichText.SelectionStart = this.RichText.GetFirstCharIndexOfCurrentLine();
            this.RichText.SelectionLength = this.RichText.Lines[line].Length;
            this.RichText.SelectionColor = Color.Black;
            this.RichText.SelectionStart = oldstart;
            this.RichText.SelectionLength = oldlen;
            this.RichText.LockPaint = false;
            MarkKeyWords(false);
        }

        void SetLineNo(bool addNewLine = false)
        {
            int line1 = CurrentClientScreenStartLine + 1;
            int line2 = CurrentClientScreentEndLine + 1;
            if (addNewLine)
                line2 += 1;
            //if (line1 == this.ScaleNos.FirstLine && line2 == this.ScaleNos.LastLine)
            //    return;
            Dictionary<int, Point> nos = new Dictionary<int, Point>();
            int offset = 0;
            int strLen = this.RichText.GetCharIndexFromPosition(new Point(0, 0)) + 1;
            int linesLen = RichText.Lines.Length;
            for (int i = line1; i <= line2 && i <= linesLen; i++)
            {
                //要算上一个换行符
                var curlen = RichText.Lines[i - 1].Length;
                if (curlen == 0)
                {
                    if (i == line1)
                    {
                        continue;
                    }
                    Point p = new Point(2, 0);
                    p.Y = offset + (offset == 0 ? 0 : this.Font.Height) + 1;
                    offset = p.Y;
                    nos.Add(i, p);
                    strLen += 1;
                }
                else
                {
                    Point p = this.RichText.GetPositionFromCharIndex(strLen);
                    offset = p.Y;
                    p.X = 2;
                    nos.Add(i, p);
                    strLen += RichText.Lines[i - 1].Length + 1;
                }
            }
            this.ScaleNos.LineNos = nos;
        }

        void RichText_VScroll(object sender, EventArgs e)
        {
            if (RichText.SelectionLength == 0)
            {
                var currline = RichText.GetLineFromCharIndex(RichText.SelectionStart);

                if (currline > CurrentClientScreentEndLine && currline + 1 != RichText.Lines.Length)
                {
                    RichText.SelectionStart = RichText.GetFirstCharIndexFromLine(CurrentClientScreentEndLine);
                }
                else if (currline < CurrentClientScreenStartLine)
                {
                    RichText.SelectionStart = RichText.GetFirstCharIndexFromLine(CurrentClientScreenStartLine);
                }
            }
            _timer.SetTimeOutCallBack(() =>
            {
                this.Invoke(new Action<bool>(MarkKeyWords), true);
                this.Invoke(new Action(() => {

                }));
            });
        }

        public void AppendText(string text)
        {
            this.RichText.AppendText(text);
        }

        /// <summary>
        /// 当前屏幕开始行
        /// </summary>
        private int CurrentClientScreenStartLine
        {
            get
            {
                return this.RichText.GetLineFromCharIndex(this.RichText.GetCharIndexFromPosition(new Point(0, 0)));
            }
        }

        private int CurrentClientScreentEndLine
        {
            get
            {
                return this.RichText.GetLineFromCharIndex(this.RichText.GetCharIndexFromPosition(new Point(0, this.RichText.Height)));
            }
        }

        /// <summary>
        /// 分解过程
        /// </summary>
        /// <param name="express"></param>
        /// <returns></returns>
        public void MarkKeyWords(bool reSetLineNo)
        {
            try
            {
                this.RichText.SelectionChanged -= RichText_SelectionChanged;
                if (this.RichText.Lines.Length == 0)
                    return;
                int line1 = CurrentClientScreenStartLine;
                if (_lastMarketedLines == line1)
                    return;

                int line2 = CurrentClientScreentEndLine + 1;
                //if (line2 == 1)
                //{
                //    return;
                //}

                int oldStart = this.RichText.SelectionStart;
                int oldSelectLen = this.RichText.SelectionLength;

                int totalIndex = this.RichText.GetCharIndexFromPosition(new Point(0, 0));
                //if (oldStart < totalIndex)
                //{
                //    oldStart = totalIndex + RichText.Lines[line1].Length + 1;
                //    oldSelectLen = 0;
                //}
                //if (oldStart > this.RichText.GetFirstCharIndexFromLine(line2 - 1) + this.RichText.Lines[line2 - 1].Length)
                //{
                //    oldStart = this.RichText.GetFirstCharIndexFromLine(line2 - 1);
                //    oldSelectLen = 0;
                //}

                DataTable tb = CreateFatTable("pos", "len", "color");

                var linesLen = this.RichText.Lines.Length;
                for (int l = line1; l <= line2 && l < linesLen; l++)
                {
                    totalIndex = this.RichText.GetFirstCharIndexFromLine(l);
                    string express = RichText.Lines[l] + " ";

                    if (!_markedLines.Contains(l))
                    {
                        _markedLines.Add(l);

                        var nodeindex = express.IndexOf("//");
                        if (nodeindex > -1)
                        {
                            DataRow row = tb.NewRow();
                            row[0] = totalIndex + nodeindex;
                            row[1] = express.Length - nodeindex;
                            row[2] = Color.Gray;
                            tb.Rows.Add(row);
                            express = express.Substring(0, nodeindex + 1);
                        }
                        foreach (var m in this.KeyWords.MatchKeyWord(express))
                        {
                            if ((m.PostionStart == 0 || "[]{},|%#!<>:=();+-*./\r\n 　".IndexOf(express[m.PostionStart - 1]) > -1)
                                && (m.PostionEnd == express.Length - 1 || "[]{},|%#!<>:=();+-*./\r\n 　".IndexOf(express[m.PostionEnd + 1]) > -1))
                            {
                                DataRow row = tb.NewRow();
                                row[0] = totalIndex + m.PostionStart;
                                row[1] = m.KeyWordMatched.Length;
                                row[2] = m.Tag;
                                tb.Rows.Add(row);
                            }
                        }
                    }
                }

                this.RichText.LockPaint = true;
                foreach (DataRow row in tb.Rows)
                {
                    this.RichText.SelectionStart = (int)row[0];
                    this.RichText.SelectionLength = (int)row[1];
                    this.RichText.SelectionColor = (Color)row[2];
                }

                if (this.RichText.SelectionStart != oldStart)
                {
                    this.RichText.SelectionStart = oldStart;
                }
                this.RichText.SelectionLength = oldSelectLen;
                //this.RichText.SelectionColor = oldSelectColor;
                _lastMarketedLines = line1;

            }
            finally
            {
                this.RichText.LockPaint = false;
                this.RichText.SelectionChanged += RichText_SelectionChanged;
                if (reSetLineNo)
                    SetLineNo();
            }

        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.RichText.Paste();
            MarkKeyWords(true);
        }

        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return contextMenuStrip1;
            }
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.RichText.SelectAll();
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.RichText.SelectedText);
        }

        private void RichText_MouseHover(object sender, EventArgs e)
        {
        }

        private Tuple<string, string> GetTableName(string s, string defalutdb)
        {
            var arr = s.Split('.');
            if (arr.Length == 1 || (arr[arr.Length - 2].Equals("dbo", StringComparison.OrdinalIgnoreCase) && arr.Length == 2))
            {
                return new Tuple<string, string>(defalutdb.ToUpper(), arr.Last().Trim('[', ']').Trim().ToUpper());
            }
            else if (arr.Length == 3)
            {
                return new Tuple<string, string>(arr.First().Trim('[', ']').Trim().ToUpper(), arr.Last().Trim('[', ']').Trim().ToUpper());
            }
            return new Tuple<string, string>(arr.First().Trim('[', ']').Trim().ToUpper(),
                arr.Last().Trim('[', ']').Trim().ToUpper());
        }

        private void RichText_SelectionChanged(object sender, EventArgs e)
        {
            this.剪切ToolStripMenuItem.Enabled = this.SelectionLength > 0;
        }

        private void 搜索ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void TSMI_Save_Click(object sender, EventArgs e)
        {
        }

        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SelectionLength > 0)
            {
                Clipboard.SetData(DataFormats.Rtf, this.RichText.SelectedRtf);
                this.RichText.SelectedRtf = string.Empty;
            }
        }

        private void 撤消ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.RichText.CanUndo)
            {
                this.RichText.Undo();
            }
        }

        private void 重做ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.RichText.CanRedo)
            {
                this.RichText.Redo();
            }
        }

        private void TSMI_SaveAsFile_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 创建一个表格
        /// </summary>
        /// <param name="colsName">表格字段,可以加//注释</param>
        /// <returns></returns>
        public static DataTable CreateFatTable(params string[] colsName)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < colsName.Length; i++)
            {
                dt.Columns.Add(colsName[i].Split(new string[] { "//" }, StringSplitOptions.None)[0].Trim(), typeof(object));
            }

            return dt;
        }
    }
}
