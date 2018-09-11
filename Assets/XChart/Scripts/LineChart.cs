﻿
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace xchart
{
    [System.Serializable]
    public class LineData
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public string key;
        [SerializeField]
        public Color lineColor;
        [SerializeField]
        public Color pointColor;
        [SerializeField]
        public Button button;

        private List<float> _dataList = new List<float>();
        public List<float> dataList
        {
            get { return _dataList; }
        }

        private bool _visible = true;
        public bool visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                if (button)
                {
                    button.GetComponent<Image>().color = visible ? lineColor : Color.grey;
                }
            }
        }

        private int _min = 0;
        public int min
        {
            get { return _min; }
        }

        private int _max = 10;
        public int max
        {
            get { return _max; }
        }
        private int _step = 10;
        public int step
        {
            get { return _step; }
            set { _step = value; }
        }

        public void AddData(float data, int maxCount)
        {
            dataList.Add(data);
            if (dataList.Count > maxCount)
            {
                dataList.RemoveAt(0);
                UpdateMinMax();
            }
            else
            {
                if (data < _min)
                {
                    _min = (int)data;
                }
                if (data > _max)
                {
                    _max = (int)data;
                }
                CheckMax();
            }
        }

        public void ClearData()
        {
            _dataList.Clear();
        }

        public void UpdateMinMax()
        {
            _min = 0;
            _max = 4;
            foreach (var data in dataList)
            {
                if (data < _min)
                {
                    _min = (int)data;
                }
                if (data > _max)
                {
                    _max = (int)data;
                }
            }
            CheckMax();
        }

        private void CheckMax()
        {
            if (_max <= 10)
            {
                if (_max < 4) _max = 4;
            }
            else
            {
                int diff = _max % _step;
                if (diff > 1)
                {
                    _max = (_max - diff) + _step;
                }
            }
        }
    }


    public class LineChart : MaskableGraphic
    {
        private const int MAX_GRADUATION = 10;

        [SerializeField]
        private int pointWidth = 15;
        [SerializeField]
        private float lineSize = 1f;
        [SerializeField]
        private float pointSize = 1.5f;
        [SerializeField]
        private int graduationCount = 4;
        [SerializeField]
        private int graduationStep = 10;
        [SerializeField]
        private int graduationWidth = 50;

        private float arrowLen = 10;
        private float arrowSize = 6;

        [SerializeField]
        private Color backgroundColor;
        [SerializeField]
        private Font font;
        
        [SerializeField]
        private List<LineData> lineList = new List<LineData>();

        private Button btnAll;
        private bool isShowAll;
        private List<Text> graduationList = new List<Text>();
        private Dictionary<string, LineData> lineMap = new Dictionary<string, LineData>();
        private float lastMaxData = 0;
        private float lastChartHig = 0;
        private float lastGraduationWid = 0;

        protected override void Awake()
        {
            base.Awake();
            InitGraduation();
            InitLineButton();
            InitHideAndShowButton();
            for (int i = 0; i < lineList.Count; i++)
            {
                LineData line = lineList[i];
                line.dataList.Clear();
                if (line.button)
                {
                    Color bcolor = line.visible ? line.lineColor : Color.grey;
                    line.button.GetComponent<Image>().color = bcolor;
                    line.button.GetComponentInChildren<Text>().text = line.name;
                    line.button.onClick.AddListener(delegate ()
                    {
                        OnClickButton(line.key);
                    });
                }
                AddLineToLineMap(line);
            }
        }

        private void InitGraduation()
        {
            float chartHigh = rectTransform.sizeDelta.y;
            float graduationHig = chartHigh / graduationCount;
            
            for (int i = 0; i < MAX_GRADUATION; i++)
            {
                if (i >= graduationCount + 1)
                {
                    if (transform.Find("graduation" + i))
                    {
                        transform.Find("graduation" + i).gameObject.SetActive(false);
                    }
                }
                else
                {
                    GameObject txtObj;
                    if (transform.Find("graduation" + i))
                    {
                        txtObj = transform.Find("graduation" + i).gameObject;
                        txtObj.SetActive(true);
                    }
                    else
                    {
                        txtObj = new GameObject();
                        txtObj.name = "graduation" + i;
                        txtObj.transform.parent = transform;
                        txtObj.transform.localPosition = Vector3.zero;
                        txtObj.transform.localScale = Vector3.one;
                        Text txt = txtObj.AddComponent<Text>();
                        txt.font = font;
                        txt.text = (i * 100).ToString();
                        txt.alignment = TextAnchor.MiddleRight;
                    }
                    RectTransform rect = txtObj.GetComponent<RectTransform>();
                    rect.anchorMax = Vector2.zero;
                    rect.anchorMin = Vector2.zero;
                    rect.sizeDelta = new Vector2(50, 20);
                    rect.localPosition = new Vector3(-33, i * graduationHig, 0);
                    graduationList.Add(txtObj.GetComponent<Text>());
                }
            }
        }

        private void InitLineButton()
        {
            float chartHigh = rectTransform.sizeDelta.y;
            for (int i = 0; i < lineList.Count; i++)
            {
                if (lineList[i].button) continue;

                GameObject btnObj;
                if (transform.Find("button" + i))
                {
                    btnObj = transform.Find("button" + i).gameObject;
                }
                else
                {
                    btnObj = new GameObject();
                    btnObj.name = "button" + i;
                    btnObj.transform.parent = transform;
                    btnObj.transform.localPosition = Vector3.zero;
                    btnObj.transform.localScale = Vector3.one;
                    btnObj.AddComponent<Image>();
                    btnObj.AddComponent<Button>();

                    GameObject txtObj = new GameObject();
                    txtObj.name = "Text";
                    txtObj.transform.parent = btnObj.transform;
                    txtObj.transform.localPosition = Vector3.zero;
                    txtObj.transform.localScale = Vector3.one;
                    txtObj.AddComponent<Text>();
                    Text txt = txtObj.GetComponent<Text>();
                    txt.font = font;
                    txt.text = (i * 100).ToString();
                    txt.alignment = TextAnchor.MiddleCenter;
                }
                RectTransform rect = btnObj.GetComponent<RectTransform>();
                if (rect == null)
                {
                    rect = btnObj.AddComponent<RectTransform>();
                }
                rect.anchorMax = Vector2.zero;
                rect.anchorMin = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.sizeDelta = new Vector2(50, 20);
                rect.localPosition = new Vector3(i*50, chartHigh + 30, 0);
                Button btn = btnObj.GetComponent<Button>();
                lineList[i].button = btn;
            }
        }

        private void InitHideAndShowButton()
        {
            if (lineList.Count <= 0) return;
            GameObject btnObj;
            float chartHigh = rectTransform.sizeDelta.y;
            if (transform.Find("buttonall"))
            {
                btnObj = transform.Find("buttonall").gameObject;
            }
            else
            {
                btnObj = new GameObject();
                btnObj.name = "buttonall";
                btnObj.transform.parent = transform;
                btnObj.transform.localPosition = Vector3.zero;
                btnObj.transform.localScale = Vector3.one;
                btnObj.AddComponent<Image>();
                btnObj.AddComponent<Button>();

                GameObject txtObj = new GameObject();
                txtObj.name = "Text";
                txtObj.transform.parent = btnObj.transform;
                txtObj.transform.localPosition = Vector3.zero;
                txtObj.transform.localScale = Vector3.one;
                txtObj.AddComponent<Text>();
                Text txt = txtObj.GetComponent<Text>();
                txt.font = font;
                txt.text = "SHOW";
                txt.alignment = TextAnchor.MiddleCenter;
            }
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = btnObj.AddComponent<RectTransform>();
            }
            rect.anchorMax = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = new Vector2(graduationWidth, 20);
            rect.localPosition = new Vector3(-graduationWidth, chartHigh + 30, 0);
            btnAll = btnObj.GetComponent<Button>();
            btnAll.GetComponent<Image>().color = backgroundColor;
            btnAll.onClick.AddListener(delegate ()
            {
                isShowAll = !isShowAll;
                btnAll.GetComponentInChildren<Text>().text = isShowAll ? "HIDE" : "SHOW";
                foreach(var line in lineList)
                {
                    line.visible = isShowAll;
                }
            });
        }

        private void Update()
        {
            CheckLineSizeChange();
        }

        void OnClickButton(string key)
        {
            LineData line = lineMap[key];
            line.visible = !line.visible;
            if (line.visible)
            {
                line.step = graduationStep;
                line.UpdateMinMax();
            }
            CheckMaxDataChange();
            UpdateMesh();
        }

        private void AddLineToLineMap(LineData line)
        {
            if (lineMap.ContainsKey(line.key))
            {
                Debug.LogError("LineChart:line key is duplicated:" + line.key);
            }
            else
            {
                lineMap[line.key] = line;
            }
        }

        private float GetAllLineMax()
        {
            float max = 4;
            foreach (var line in lineList)
            {
                if (line.visible && line.max > max)
                {
                    max = line.max;
                }
            }
            return max;
        }

        public int GetMaxPointCount()
        {
            Vector2 size = rectTransform.sizeDelta;
            int max = (int)(size.x / pointWidth);
            return max;
        }

        public void AddLine(string key, string name, Color lineColor, Color pointColor)
        {
            LineData line = new LineData();
            line.key = key;
            line.name = name;
            line.lineColor = lineColor;
            line.pointColor = pointColor;
            lineList.Add(line);
            AddLineToLineMap(line);
        }

        public void AddPoint(string key, float point)
        {
            if (!lineMap.ContainsKey(key))
            {
                Debug.LogError("LineChart:not contain line key:" + key);
                return;
            }
            LineData line = lineMap[key];
            line.AddData(point, GetMaxPointCount());
            UpdateMesh();
            CheckMaxDataChange();
        }

        public void ResetDataStart()
        {
            foreach (var line in lineList)
            {
                line.ClearData();
            }
        }

        public void ResetData(string key, float data)
        {
            if (!lineMap.ContainsKey(key))
            {
                Debug.LogError("LineChart:not contain line key:" + key);
                return;
            }
            LineData line = lineMap[key];
            line.AddData(data, GetMaxPointCount());
        }

        public void ResetDataEnd()
        {
            foreach (var line in lineList)
            {
                line.UpdateMinMax();
            }
            UpdateMesh();
            CheckMaxDataChange();
        }

        private void UpdateMesh()
        {
            Vector2 size = rectTransform.sizeDelta;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (int)size.x - 1);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (int)size.x);
        }

        private void CheckLineSizeChange()
        {
            float chartHig = rectTransform.sizeDelta.y;
            if (lastChartHig != chartHig)
            {
                lastChartHig = chartHig;
                //update graduation pos
                for (int i = 0; i < graduationList.Count; i++)
                {
                    Vector3 pos = graduationList[i].rectTransform.localPosition;
                    float posY = lastChartHig * i / (graduationList.Count - 1);
                    graduationList[i].rectTransform.localPosition = new Vector3(pos.x, posY, pos.z);
                }
                //update line button pos
                btnAll.transform.localPosition = new Vector3(-graduationWidth, chartHig + 30, 0);
                for (int i = 0; i < lineList.Count; i++)
                {
                    LineData line = lineList[i];
                    if (line.button)
                    {
                        line.button.transform.localPosition = new Vector3(i * 50, chartHig + 30, 0);
                    }
                }
            }
            if(lastGraduationWid != graduationWidth)
            {
                lastGraduationWid = graduationWidth;
                btnAll.GetComponent<RectTransform>().sizeDelta = new Vector2(graduationWidth, 20);
                btnAll.transform.localPosition = new Vector3(-graduationWidth, chartHig + 30, 0);
            }
        }

        private void CheckMaxDataChange()
        {
            float dataMax = GetAllLineMax();
            if (lastMaxData != dataMax)
            {
                lastMaxData = dataMax;
                for (int i = 0; i < graduationList.Count; i++)
                {
                    graduationList[i].text = ((int)(dataMax * i / graduationList.Count)).ToString();
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            float chartHigh = rectTransform.sizeDelta.y;
            int chartWid = (int)(rectTransform.sizeDelta.x / pointWidth) * pointWidth;
            float dataMax = GetAllLineMax();
            // draw bg
            Vector3 p1 = new Vector3(-graduationWidth, chartHigh + 30);
            Vector3 p2 = new Vector3(chartWid + 50, chartHigh + 30);
            Vector3 p3 = new Vector3(chartWid + 50, -20);
            Vector3 p4 = new Vector3(-graduationWidth, -20);
            DrawCube(vh, p1, p2, p3, p4, backgroundColor);
            // draw coordinate
            Vector3 coordZero = Vector3.zero;
            DrawLine(vh, new Vector3(chartWid + 5, -5), new Vector3(chartWid + 5, chartHigh + 0.5f), 1, Color.grey);
            // draw graduation
            for (int i = 0; i < graduationList.Count; i++)
            {
                Vector3 sp = new Vector3(-5, chartHigh * i / (graduationList.Count - 1));
                Vector3 ep = new Vector3(chartWid + 5, chartHigh * i / (graduationList.Count - 1));
                DrawLine(vh, sp, ep, 0.5f, Color.grey);
            }

            // draw line
            for (int index = 0; index < lineList.Count; index++)
            {
                LineData line = lineList[index];
                if (!line.visible) continue;
                Vector3 lp = Vector3.zero;
                Vector3 np = Vector3.zero;

                for (int i = 0; i < line.dataList.Count; i++)
                {
                    float data = line.dataList[i] * chartHigh / dataMax;
                    np = new Vector3(i * pointWidth, data);
                    if (i > 0)
                    {
                        DrawLine(vh, lp, np, lineSize, line.lineColor);
                    }
                    lp = np;
                }

                // draw point
                for (int i = 0; i < line.dataList.Count; i++)
                {
                    UIVertex[] quadverts = new UIVertex[4];
                    float data = line.dataList[i] * chartHigh / dataMax;
                    Vector3 p = new Vector3(i * pointWidth, data);
                    DrawCube(vh, p, pointSize, line.pointColor);
                }
            }

            //draw x,y axis
            float xLen = chartWid + 25;
            float yLen = chartHigh + 15;
            float xPos = 0;
            float yPos = -5;
            DrawLine(vh, new Vector3(xPos, yPos - 1.5f), new Vector3(xPos, yLen), 1.5f, Color.white);
            DrawLine(vh, new Vector3(xPos, yPos), new Vector3(xLen, yPos), 1.5f, Color.white);
            //draw arrows
            DrawTriangle(vh, new Vector3(xPos - arrowSize, yLen - arrowLen), new Vector3(xPos, yLen + 4), new Vector3(xPos + arrowSize, yLen - arrowLen), Color.white);
            DrawTriangle(vh, new Vector3(xLen - arrowLen, yPos + arrowSize), new Vector3(xLen + 4, yPos), new Vector3(xLen - arrowLen, yPos - arrowSize), Color.white);
        }

        private void DrawLine(VertexHelper vh, Vector3 p1, Vector3 p2, float size, Color color)
        {
            Vector3 v = Vector3.Cross(p2 - p1, Vector3.forward).normalized * size;

            UIVertex[] vertex = new UIVertex[4];
            vertex[0].position = p1 + v;
            vertex[1].position = p2 + v;
            vertex[2].position = p2 - v;
            vertex[3].position = p1 - v;
            for (int j = 0; j < 4; j++)
            {
                vertex[j].color = color;
                vertex[j].uv0 = Vector2.zero;
            }
            vh.AddUIVertexQuad(vertex);
        }

        private void DrawCube(VertexHelper vh, Vector3 p, float size, Color color)
        {
            UIVertex[] vertex = new UIVertex[4];
            vertex[0].position = new Vector3(p.x - pointSize, p.y - pointSize);
            vertex[1].position = new Vector3(p.x + pointSize, p.y - pointSize);
            vertex[2].position = new Vector3(p.x + pointSize, p.y + pointSize);
            vertex[3].position = new Vector3(p.x - pointSize, p.y + pointSize);
            for (int j = 0; j < 4; j++)
            {
                vertex[j].color = color;
                vertex[j].uv0 = Vector2.zero;
            }
            vh.AddUIVertexQuad(vertex);
        }

        private void DrawCube(VertexHelper vh, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color)
        {
            UIVertex[] vertex = new UIVertex[4];
            vertex[0].position = p1;
            vertex[1].position = p2;
            vertex[2].position = p3;
            vertex[3].position = p4;
            for (int j = 0; j < 4; j++)
            {
                vertex[j].color = color;
                vertex[j].uv0 = Vector2.zero;
            }
            vh.AddUIVertexQuad(vertex);
        }

        private void DrawTriangle(VertexHelper vh, Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            List<UIVertex> vertexs = new List<UIVertex>();
            vh.GetUIVertexStream(vertexs);
            DrawTriangle(vh, vertexs, p1, p2, p3, color);
        }

        private void DrawTriangle(VertexHelper vh, List<UIVertex> vertexs, Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            UIVertex v1 = new UIVertex();
            v1.position = p1;
            v1.color = color;
            v1.uv0 = Vector3.zero;
            vertexs.Add(v1);
            UIVertex v2 = new UIVertex();
            v2.position = p2;
            v2.color = color;
            v2.uv0 = Vector3.zero;
            vertexs.Add(v2);
            UIVertex v3 = new UIVertex();
            v3.position = p3;
            v3.color = color;
            v3.uv0 = Vector3.zero;
            vertexs.Add(v3);
            vh.AddUIVertexTriangleStream(vertexs);
        }

        private void DrawCricle(VertexHelper vh, Vector3 p, float radius, Color color, int segments)
        {
            List<UIVertex> vertexs = new List<UIVertex>();
            vh.GetUIVertexStream(vertexs);
            float angle = 2 * Mathf.PI / segments;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            float cx = radius, cy = 0;

            List<UIVertex> vs = new List<UIVertex>();

            segments--;
            Vector3 p2, p3;
            for (int i = 0; i < segments; i++)
            {
                p2 = new Vector3(p.x + cx, p.y + cy);
                float temp = cx;
                cx = cos * cx - sin * cy;
                cy = sin * temp + cos * cy;
                p3 = new Vector3(p.x + cx, p.y + cy);
                DrawTriangle(vh, vertexs, p, p2, p3, color);
            }
            p2 = new Vector3(p.x + cx, p.y + cy);
            cx = radius;
            cy = 0;
            p3 = new Vector3(p.x + cx, p.y + cy);
            DrawTriangle(vh, vertexs, p, p2, p3, color);
        }  
    }
}