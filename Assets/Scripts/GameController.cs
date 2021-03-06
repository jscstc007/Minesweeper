﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour {

    private const int BG_WIDTH = 1280;
    private const int BG_HEIGHT = 720;
    private const int GRID_LEN = 80;
    private const int WIDTH_LEN = (BG_WIDTH / GRID_LEN);
    private const int HEIGHT_LEN = (BG_HEIGHT / GRID_LEN);
    private const int MAX_GRID_NUM = WIDTH_LEN * HEIGHT_LEN;

    private Transform root;

    private Transform bgP;
    private GameObject item;

    private Image[] gridIs = new Image[MAX_GRID_NUM];
    private Text[] gridTs = new Text[MAX_GRID_NUM];
    private Text[] gridFlagTs = new Text[MAX_GRID_NUM];

    /// <summary>
    /// 是否展示地雷标记(用于测试)
    /// </summary>
    private bool showMine = true;

    /// <summary>
    /// 是否游戏已结束
    /// </summary>
    private bool PlayEnd = false;

    /// <summary>
    /// 地雷位置数据
    /// </summary>
    private List<int> MineData = new List<int>();

    private List<int> TempGridData = new List<int>();//临时数据 用于生成随即地雷数据

    /// <summary>
    /// 旗帜位置数据
    /// </summary>
    private List<int> FlagData = new List<int>();

    /// <summary>
    /// 当前已激活的位置信息
    /// </summary>
    private List<int> ShowData = new List<int>();

    /// <summary>
    /// 地雷总数量
    /// </summary>
    private const int MINE_NUM = 40;

    private void Awake()
    {
        root = GameObject.Find("/UI").transform;
        bgP = root.Find("BG_P");
        item = root.Find("Item").gameObject;
    }

    // Use this for initialization
    void Start () {
        //重置游戏
        ResetGame();
        //注册UI
        root.Find("Reset_B").GetComponent<Button>().onClick.AddListener(ResetGame);   
    }

    /// <summary>
    /// 重置游戏
    /// </summary>
    private void ResetGame ()
    {
        //清空状态与数据
        PlayEnd = false;
        
        MineData.Clear();
        TempGridData.Clear();

        FlagData.Clear();

        ShowData.Clear();

        int childNum = bgP.childCount;
        //初始化/重置格子信息
        if (childNum == 0)
        {
            for (int i = 0;i < MAX_GRID_NUM; i ++)
            {
                GameObject obj = Instantiate(item);
                obj.name = i.ToString();
                obj.transform.SetParent(bgP);
                int index = i;
                gridIs[index] = obj.transform.Find("Mask").GetComponent<Image>();
                gridTs[index] = obj.transform.Find("Info").GetComponent<Text>();
                gridFlagTs[index] = obj.transform.Find("Flag").GetComponent<Text>();
                //obj.transform.Find("Index").GetComponent<Text>().text = index.ToString();
                obj.GetComponent<ButtonClickExpand>().leftClick.AddListener(() => { OnLeftClickGrid(index); });
                obj.GetComponent<ButtonClickExpand>().rightClick.AddListener(() => { OnRightClickGrid(index); });
                obj.SetActive(true);
            }
        }
        else
        {
            for (int i = 0;i < MAX_GRID_NUM ; i ++)
            {
                if (showMine)
                {
                    gridIs[i].enabled = false;
                }
                
                gridTs[i].text = string.Empty;
                gridFlagTs[i].text = string.Empty;
            }
        }
       
        //随即生成地雷数据
        for (int i = 0;i < MAX_GRID_NUM; i ++)
        {
            TempGridData.Add(i);
        }
        for (int i = 0; i < MINE_NUM;i ++)
        {
            int tempCount = TempGridData.Count;
            int randomIndex = Random.Range(0, tempCount);

            int mineIndex = TempGridData[randomIndex];
            TempGridData.RemoveAt(randomIndex);
            MineData.Add(mineIndex);

            if (showMine)
            {
                gridIs[mineIndex].enabled = true;
            }
        }

        //重置旗帜数量显示
        root.Find("Flag_T").GetComponent<Text>().text = string.Format("{0}/{1}",FlagData.Count,MINE_NUM); 

        Debug.Log("Reset Game");
    }

    /// <summary>
    /// 左键点击Grid
    /// </summary>
    /// <param name="gridIndex"></param>
    private void OnLeftClickGrid(int gridIndex)
    {
        if (PlayEnd)
        {
            Debug.Log("游戏结束 请重新开始游戏");

            return;
        }

        Debug.Log("OnLeftClickGrid ---- gridIndex:" + gridIndex);

        //如果该点已被Flag 则反转Flag
        if (FlagData.Contains(gridIndex))
        {
            FlagData.Remove(gridIndex);

            gridFlagTs[gridIndex].text = string.Empty;

            //重置旗帜数量显示
            root.Find("Flag_T").GetComponent<Text>().text = string.Format("{0}/{1}", FlagData.Count, MINE_NUM);

            //如果旗帜数据与地雷数据一致 则获得游戏胜利
            if (CheckIfWin())
            {
                PlayEnd = true;
                //获胜了！
                Debug.Log("你胜利了 请重新开始游戏");
            }
        }

        //该点为地雷点
        if (MineData.Contains(gridIndex))
        {
            gridTs[gridIndex].text = "Boom";

            PlayEnd = true;
        }
        else
        {
            UpdateGridUI(gridIndex);
        }
    }

    /// <summary>
    /// 右键点击Grid
    /// </summary>
    private void OnRightClickGrid (int gridIndex)
    {
        if (PlayEnd)
        {
            
        }

        //如果该点已经显示了 则无效
        if (ShowData.Contains(gridIndex))
        {
            Debug.Log("该点已展示 请选择其他位置");

            return;
        }

        Debug.Log("OnRightClickGrid ---- gridIndex:" + gridIndex);

        //反转旗帜
        if (FlagData.Contains(gridIndex))
        {
            FlagData.Remove(gridIndex);

            gridFlagTs[gridIndex].text = string.Empty;
        }
        else
        {
            FlagData.Add(gridIndex);

            gridFlagTs[gridIndex].text = "Flag";
        }

        //重置旗帜数量显示
        root.Find("Flag_T").GetComponent<Text>().text = string.Format("{0}/{1}", FlagData.Count, MINE_NUM);

        //如果旗帜数据与地雷数据一致 则获得游戏胜利
        if (CheckIfWin())
        {
            PlayEnd = true;
            //获胜了！
            Debug.Log("你胜利了 请重新开始游戏");
        }
    }

    private bool CheckIfWin ()
    {
        bool isWin = false;
        //如果旗帜数据与地雷数据一致 则获得游戏胜利
        if (FlagData.Count == MINE_NUM)
        {
            isWin = true;
            for (int i = 0; i < MINE_NUM; i++)
            {
                if (!FlagData.Contains(MineData[i]))
                {
                    isWin = false;
                    break;
                }
            }
        }
        return isWin;
    }

    private void UpdateGridUI (int gridIndex)
    {
        if (ShowData.Contains(gridIndex))
        {
            //已触发过的不再触发
        }
        else
        {
            Debug.Log("UpdateGridUI:" + gridIndex);

            ShowData.Add(gridIndex);

            //该点上下左右为地雷点的总数量
            int neighborMineNum = 0;

            int upIndex = gridIndex - WIDTH_LEN;
            int upLeftIndex = gridIndex - WIDTH_LEN - 1;
            int upRightIndex = gridIndex - WIDTH_LEN + 1;
            int downIndex = gridIndex + WIDTH_LEN;
            int downLeftIndex = gridIndex + WIDTH_LEN - 1;
            int downRightIndex = gridIndex + WIDTH_LEN + 1;
            int leftIndex = gridIndex - 1;
            int rightIndex = gridIndex + 1;
            //修订处于最左边的情况
            if (gridIndex % WIDTH_LEN == 0)
            {
                upLeftIndex = -1;
                leftIndex = -1;
                downLeftIndex = -1;
            }
            //修订处于最右边的情况
            if ((gridIndex+1) % WIDTH_LEN == 0)
            {
                upRightIndex = -1;
                rightIndex = -1;
                downRightIndex = -1;
            }

            if (CheckIsInGrid(upIndex) && CheckIsMine(upIndex))
            {
                neighborMineNum += 1;
                //Debug.Log("Mine -- UP:" + gridIndex);
            }
            if (CheckIsInGrid(upLeftIndex) && CheckIsMine(upLeftIndex))
            {
                neighborMineNum += 1;
                //Debug.Log("Mine -- UP LEFT:" + gridIndex);
            }
            if (CheckIsInGrid(upRightIndex) && CheckIsMine(upRightIndex))
            {
                neighborMineNum += 1;
                //Debug.Log("Mine -- UP RIGHT:" + gridIndex);
            }
            if (CheckIsInGrid(downIndex) && CheckIsMine(downIndex))
            {
                neighborMineNum += 1;
                //Debug.Log("Mine -- DOWN:" + gridIndex);
            }
            if (CheckIsInGrid(downLeftIndex) && CheckIsMine(downLeftIndex))
            {
                neighborMineNum += 1;
                //Debug.Log("Mine -- DOWN LEFT:" + gridIndex);
            }
            if (CheckIsInGrid(downRightIndex) && CheckIsMine(downRightIndex))
            {
                neighborMineNum += 1;
                //Debug.Log("Mine -- DOWN RIGHT:" + gridIndex);
            }
            if (CheckIsInGrid(leftIndex) && CheckIsMine(leftIndex))
            {
                neighborMineNum += 1;
                //Debug.Log("Mine -- LEFT:" + gridIndex);
            }
            if (CheckIsInGrid(rightIndex) && CheckIsMine(rightIndex))
            {
                neighborMineNum += 1;
                //Debug.Log("Mine -- RIGHT:" + gridIndex);
            }

            //更新该点信息
            gridTs[gridIndex].text = neighborMineNum.ToString();

            //如果该点为0 则对其周围进行相同处理
            if (neighborMineNum == 0)
            {
                if (CheckIsInGrid(upIndex))
                {
                    UpdateGridUI(upIndex);
                }
                if (CheckIsInGrid(upLeftIndex))
                {
                    UpdateGridUI(upLeftIndex);
                }
                if (CheckIsInGrid(upRightIndex))
                {
                    UpdateGridUI(upRightIndex);
                }
                if (CheckIsInGrid(downIndex))
                {
                    UpdateGridUI(downIndex);
                }
                if (CheckIsInGrid(upLeftIndex))
                {
                    UpdateGridUI(upLeftIndex);
                }
                if (CheckIsInGrid(downRightIndex))
                {
                    UpdateGridUI(downRightIndex);
                }
                if (CheckIsInGrid(leftIndex))
                {
                    UpdateGridUI(leftIndex);
                }
                if (CheckIsInGrid(rightIndex))
                {
                    UpdateGridUI(rightIndex);
                }
            }
        }
    }

    /// <summary>
    /// 判断一个index是否为正常范围的index
    /// </summary>
    private bool CheckIsInGrid (int index)
    {
        return (0 <= index && index < MAX_GRID_NUM);
    }
    /// <summary>
    /// 判断一个index是否为地雷
    /// </summary>
    private bool CheckIsMine (int index)
    {
        return MineData.Contains(index);
    }
}
