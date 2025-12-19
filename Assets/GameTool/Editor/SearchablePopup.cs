using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameTool.Editor
{
    /// <summary>
    /// A popup window that displays a list of options and may use a search
    /// string to filter the displayed content. 
    /// </summary>
    public class SearchablePopup : PopupWindowContent
    {
        #region -- Constants --------------------------------------------------

        /// <summary> Height of each element in the popup list. </summary>
        private const float ROW_HEIGHT = 16.0f;

        /// <summary> How far to indent list entries. </summary>
        private const float ROW_INDENT = 8.0f;

        /// <summary> Name to use for the text field for search. </summary>
        private const string SEARCH_CONTROL_NAME = "EnumSearchText";

        #endregion -- Constants -----------------------------------------------

        #region -- Static Functions -------------------------------------------

        /// <summary> Show a new SearchablePopup. </summary>
        /// <param name="activatorRect">
        /// Rectangle of the button that triggered the popup.
        /// </param>
        /// <param name="options">List of strings to choose from.</param>
        /// <param name="current">
        /// Index of the currently selected string.
        /// </param>
        /// <param name="onSelectionMade">
        /// Callback to trigger when a choice is made.
        /// </param>
        public static void Show(Rect activatorRect, string[] options, int current, Action<int> onSelectionMade,
            bool isStringPopup = false)
        {
            SearchablePopup win =
                new SearchablePopup(options, current, onSelectionMade, isStringPopup);
            PopupWindow.Show(activatorRect, win);
        }

        /// <summary>
        /// Force the focused window to redraw. This can be used to make the
        /// popup more responsive to mouse movement.
        /// </summary>
        private static void Repaint()
        {
            EditorWindow.focusedWindow.Repaint();
        }

        /// <summary> Draw a generic box. </summary>
        /// <param name="rect">Where to draw.</param>
        /// <param name="tint">Color to tint the box.</param>
        private static void DrawBox(Rect rect, Color tint)
        {
            Color c = GUI.color;
            GUI.color = tint;
            GUI.Box(rect, "", Selection);
            GUI.color = c;
        }

        #endregion -- Static Functions ----------------------------------------

        #region -- Helper Classes ---------------------------------------------

        /// <summary>
        /// Stores a list of strings and can return a subset of that list that
        /// matches a given filter string.
        /// </summary>
        private class FilteredList
        {
            /// <summary>
            /// An entry in the filtererd list, mapping the text to the
            /// original index.
            /// </summary>
            public struct Entry
            {
                public int Index;
                public string Text;
            }

            /// <summary> All posibile items in the list. </summary>
            private readonly string[] allItems;

            /// <summary> Create a new filtered list. </summary>
            /// <param name="items">All The items to filter.</param>
            public FilteredList(string[] items)
            {
                allItems = items;
                Entries = new List<Entry>();
                UpdateFilter("");
            }

            /// <summary> The current string filtering the list. </summary>
            public string Filter { get; private set; }

            /// <summary> All valid entries for the current filter. </summary>
            public List<Entry> Entries { get; private set; }

            /// <summary> Total possible entries in the list. </summary>
            public int MaxLength
            {
                get { return allItems.Length; }
            }

            /// <summary>
            /// Sets a new filter string and updates the Entries that match the
            /// new filter if it has changed.
            /// </summary>
            /// <param name="filter">String to use to filter the list.</param>
            /// <returns>
            /// True if the filter is updated, false if newFilter is the same
            /// as the current Filter and no update is necessary.
            /// </returns>
            public bool UpdateFilter(string filter)
            {
                if (string.Equals(Filter, filter, StringComparison.Ordinal))
                    return false;

                Filter = filter ?? string.Empty;
                Entries.Clear();

                // rỗng => show toàn bộ
                if (string.IsNullOrWhiteSpace(Filter))
                {
                    for (int i = 0; i < allItems.Length; i++)
                        Entries.Add(new Entry { Index = i, Text = allItems[i] });
                    return true;
                }

                var results = new List<(Entry e, float score, bool exact)>(allItems.Length);
                var buf = new List<int>(16);
                string q = Filter;

                for (int i = 0; i < allItems.Length; i++)
                {
                    string cand = allItems[i];
                    bool exact = string.Equals(cand, q, StringComparison.OrdinalIgnoreCase);

                    if (TryFuzzyScore(q, cand, buf, out float s))
                    {
                        results.Add((new Entry { Index = i, Text = cand }, s, exact));
                    }
                    else
                    {
                        // fallback substring search (case-insensitive)
                        if (cand.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // cho điểm thấp hơn fuzzy để không chen lấn
                            float fallbackScore = 0.5f;
                            results.Add((new Entry { Index = i, Text = cand }, fallbackScore, exact));
                        }
                    }
                }

                // sort: exact > score > length > alpha
                results.Sort((a, b) =>
                {
                    int cmp = b.exact.CompareTo(a.exact);
                    if (cmp != 0) return cmp;
                    cmp = b.score.CompareTo(a.score);
                    if (cmp != 0) return cmp;
                    cmp = a.e.Text.Length.CompareTo(b.e.Text.Length);
                    if (cmp != 0) return cmp;
                    return string.CompareOrdinal(a.e.Text, b.e.Text);
                });

                foreach (var r in results)
                    Entries.Add(r.e);

                return true;

                // ===== helpers =====
                static bool TryFuzzyScore(string query, string candidate, List<int> pos, out float score)
                {
                    pos.Clear();
                    if (query.Length == 0)
                    {
                        score = 0;
                        return true;
                    }

                    if (candidate.Length == 0)
                    {
                        score = float.NegativeInfinity;
                        return false;
                    }

                    int qi = 0, ci = 0, last = -2;
                    score = 0f;

                    while (qi < query.Length && ci < candidate.Length)
                    {
                        char qc = query[qi], cc = candidate[ci];
                        if (char.ToLowerInvariant(qc) == char.ToLowerInvariant(cc))
                        {
                            float s = 1f;
                            if (IsBoundary(candidate, ci)) s += 3f;
                            if (qc == cc) s += 0.25f;
                            if (last == ci - 1) s += 1.5f;
                            s += Math.Max(0f, 1f - (ci / (float)candidate.Length)) * 0.5f;

                            score += s;
                            pos.Add(ci);
                            last = ci;
                            qi++;
                            ci++;
                        }
                        else
                        {
                            score -= 0.05f;
                            ci++;
                        }
                    }

                    if (qi != query.Length) return false;

                    for (int i = 1; i < pos.Count; i++)
                    {
                        int gap = pos[i] - pos[i - 1] - 1;
                        if (gap > 0) score -= 0.02f * gap;
                    }

                    return true;
                }

                static bool IsBoundary(string s, int index)
                {
                    if (index == 0) return true;
                    char prev = s[index - 1], cur = s[index];
                    if (prev == '_' || prev == '-' || prev == ' ' || prev == '/' || prev == '\\' || prev == '.') return true;
                    if (char.IsLower(prev) && char.IsUpper(cur)) return true;
                    if (char.IsDigit(prev) && !char.IsDigit(cur)) return true;
                    return false;
                }
            }
        }

        #endregion -- Helper Classes ------------------------------------------

        #region -- Private Variables ------------------------------------------

        /// <summary> Callback to trigger when an item is selected. </summary>
        private readonly Action<int> onSelectionMade;

        private readonly bool isStringPopup;

        /// <summary>
        /// Index of the item that was selected when the list was opened.
        /// </summary>
        private readonly int currentIndex;

        /// <summary>
        /// Container for all available options that does the actual string
        /// filtering of the content.  
        /// </summary>
        private readonly FilteredList list;

        /// <summary> Scroll offset for the vertical scroll area. </summary>
        private Vector2 scroll;

        /// <summary>
        /// Index of the item under the mouse or selected with the keyboard.
        /// </summary>
        private int hoverIndex;

        /// <summary>
        /// An item index to scroll to on the next draw.
        /// </summary>
        private int scrollToIndex;

        /// <summary>
        /// An offset to apply after scrolling to scrollToIndex. This can be
        /// used to control if the selection appears at the top, bottom, or
        /// center of the popup.
        /// </summary>
        private float scrollOffset;

        #endregion -- Private Variables ---------------------------------------

        #region -- GUI Styles -------------------------------------------------

        // GUIStyles implicitly cast from a string. This triggers a lookup into
        // the current skin which will be the editor skin and lets us get some
        // built-in styles.

        private static GUIStyle SearchBox = "ToolbarSeachTextField";
        private static GUIStyle CancelButton = "ToolbarSeachCancelButton";
        private static GUIStyle DisabledCancelButton = "ToolbarSeachCancelButtonEmpty";
        private static GUIStyle Selection = "SelectionRect";

        #endregion -- GUI Styles ----------------------------------------------


        #region -- Initialization ---------------------------------------------

        private SearchablePopup(string[] names, int currentIndex, Action<int> onSelectionMade, bool isStringPopup)
        {
            list = new FilteredList(names);
            this.currentIndex = currentIndex;
            this.onSelectionMade = onSelectionMade;
            this.isStringPopup = isStringPopup;

            hoverIndex = currentIndex;
            scrollToIndex = currentIndex;
            scrollOffset = GetWindowSize().y - ROW_HEIGHT * 2;
        }

        #endregion -- Initialization ------------------------------------------

        #region -- PopupWindowContent Overrides -------------------------------

        public override void OnOpen()
        {
            base.OnOpen();
            // Force a repaint every frame to be responsive to mouse hover.
            EditorApplication.update += Repaint;
        }

        public override void OnClose()
        {
            base.OnClose();
            EditorApplication.update -= Repaint;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(base.GetWindowSize().x,
                Mathf.Min(600, list.MaxLength * ROW_HEIGHT +
                               EditorStyles.toolbar.fixedHeight));
        }

        public override void OnGUI(Rect rect)
        {
            Rect searchRect = new Rect(0, 0, rect.width, EditorStyles.toolbar.fixedHeight);
            Rect scrollRect = Rect.MinMaxRect(0, searchRect.yMax, rect.xMax, rect.yMax);

            HandleKeyboard();
            DrawSearch(searchRect);
            DrawSelectionArea(scrollRect);
        }

        #endregion -- PopupWindowContent Overrides ----------------------------

        #region -- GUI --------------------------------------------------------

        private void DrawSearch(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
                EditorStyles.toolbar.Draw(rect, false, false, false, false);

            Rect searchRect = new Rect(rect);
            searchRect.xMin += 6;
            searchRect.xMax -= 6;
            searchRect.y += 2;
            searchRect.width -= CancelButton.fixedWidth;

            GUI.FocusControl(SEARCH_CONTROL_NAME);
            GUI.SetNextControlName(SEARCH_CONTROL_NAME);
            string newText = GUI.TextField(searchRect, list.Filter, SearchBox);

            if (list.UpdateFilter(newText))
            {
                hoverIndex = 0;
                scroll = Vector2.zero;
            }

            searchRect.x = searchRect.xMax;
            searchRect.width = CancelButton.fixedWidth;

            if (string.IsNullOrEmpty(list.Filter))
                GUI.Box(searchRect, GUIContent.none, DisabledCancelButton);
            else if (GUI.Button(searchRect, "x", CancelButton))
            {
                list.UpdateFilter("");
                scroll = Vector2.zero;
            }
        }

        private void DrawSelectionArea(Rect scrollRect)
        {
            Rect contentRect = new Rect(0, 0,
                scrollRect.width - GUI.skin.verticalScrollbar.fixedWidth,
                list.Entries.Count * ROW_HEIGHT + 2);

            scroll = GUI.BeginScrollView(scrollRect, scroll, contentRect);

            Rect rowRect = new Rect(0, 0, scrollRect.width, ROW_HEIGHT);

            for (int i = 0; i < list.Entries.Count; i++)
            {
                if (scrollToIndex == i &&
                    (Event.current.type == EventType.Repaint
                     || Event.current.type == EventType.Layout))
                {
                    Rect r = new Rect(rowRect);
                    r.y += scrollOffset + 1;
                    GUI.ScrollTo(r);
                    scrollToIndex = -1;
                    scroll.x = 0;
                }

                if (rowRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseMove ||
                        Event.current.type == EventType.ScrollWheel)
                        hoverIndex = i;
                    if (Event.current.type == EventType.MouseDown)
                    {
                        onSelectionMade(list.Entries[i].Index);
                        EditorWindow.focusedWindow.Close();
                    }
                }

                DrawRow(rowRect, i);

                rowRect.y = rowRect.yMax;
            }

            GUI.EndScrollView();
        }

        private void DrawRow(Rect rowRect, int i)
        {
            if (list.Entries[i].Index == currentIndex)
                DrawBox(rowRect, Color.cyan);
            else if (i == hoverIndex)
                DrawBox(rowRect, Color.white);

            Rect labelRect = new Rect(rowRect);
            labelRect.xMin += ROW_INDENT;

            if (isStringPopup)
            {
                GUI.Label(labelRect,
                    "<color=cyan>\"</color><color=white>" + list.Entries[i].Text + "</color><color=cyan>\"</color>",
                    new GUIStyle() { richText = true });
            }
            else
            {
                GUI.Label(labelRect, list.Entries[i].Text);
            }
        }

        /// <summary>
        /// Process keyboard input to navigate the choices or make a selection.
        /// </summary>
        private void HandleKeyboard()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    hoverIndex = Mathf.Min(list.Entries.Count - 1, hoverIndex + 1);
                    Event.current.Use();
                    scrollToIndex = hoverIndex;
                    scrollOffset = ROW_HEIGHT;
                }

                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    hoverIndex = Mathf.Max(0, hoverIndex - 1);
                    Event.current.Use();
                    scrollToIndex = hoverIndex;
                    scrollOffset = -ROW_HEIGHT;
                }

                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    if (hoverIndex >= 0 && hoverIndex < list.Entries.Count)
                    {
                        onSelectionMade(list.Entries[hoverIndex].Index);
                        EditorWindow.focusedWindow.Close();
                    }
                }

                if (Event.current.keyCode == KeyCode.Escape)
                {
                    EditorWindow.focusedWindow.Close();
                }
            }
        }

        #endregion -- GUI -----------------------------------------------------
    }
}