using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MyStateCell { EXCEPTION = -1, EMPTY, ACTIVE, BUSY, RESPAWN }
public enum MyGameState { MOVE, HANDLE, WAIT }
public enum MyDirectionMouse { UNKNOWN = -1, LEFT, RIGHT, UP, DOWN }
public enum MyPowerDirection { HORIZONTAL, VERTICAL }

public struct MyPairInt
{
    public int x, y;
}
public struct MyPairActiveCell
{
    public MyActiveCell first, second;
}
public struct MyPairTypeSpawn
{
    public string typePrefab;
    public int quantity;
    public int index;
    public int indexDynamite;
    public MyPowerDirection powerDirection;

    public void MyClear() { typePrefab = null; quantity = 0; index = -1; }
}
public struct MyBufferDestroyForPowerFive
{
    private List<MyActiveCell> myListActiveCell;

    public List<MyActiveCell> myGetActiveCell { get { return myListActiveCell; } }

    public void MySetArrayActiveCell(List<MyActiveCell> _arrActiveCell)
    {
        if (myListActiveCell == null)
            myListActiveCell = new List<MyActiveCell>();

        myListActiveCell = _arrActiveCell;
    }

    public void MyClearBuffer()
    {
        if (myListActiveCell != null)
            myListActiveCell.Clear();
    }
}

public class MyMain : MonoBehaviour
{
    public readonly int myWidth = 10, myHeight = 10;
    private bool myIsHandle = false;
    private bool myIsOneCallCell = false;
    private List<MyCell> myCells;
    private List<MyActiveCell> myActiveCells;
    private List<MySpawnerGem> mySpawnerGem;
    private MyCell[,] myCells2D;

    private Coroutine myCoroutineGameState;
    private MyPairActiveCell myBufferPairActiveCell;
    private MyPairTypeSpawn myBufferTypeSpawn;
    private MyBufferDestroyForPowerFive myBufferDestroyPowerCell = new MyBufferDestroyForPowerFive();
    private event System.Action<MyGameState> MyEventStateSystem;

    public List<MyCell> MyGetCells { get { return myCells; } }
    public MyCell[,] MyGetCells2D { get { return myCells2D; } }
    public MyPairTypeSpawn myGetBufferTypeSpawn { get { return myBufferTypeSpawn; } }

    private IEnumerator Start()
    {
        myCells = new List<MyCell>(FindObjectsOfType<MyCell>());
        myCells.Sort(new MyMisc.MySortObjectId());
        myActiveCells = new List<MyActiveCell>(FindObjectsOfType<MyActiveCell>());
        myActiveCells.Sort(new MyMisc.MySortObjectId());
        mySpawnerGem = new List<MySpawnerGem>(FindObjectsOfType<MySpawnerGem>());
        myCells2D = new MyCell[myWidth, myHeight];

        for (int y = 0; y < myCells2D.GetLength(1); y++)
        {
            for (int x = 0; x < myCells2D.GetLength(0); x++)
            {
                myCells2D[x, y] = myCells[(myHeight * y) + x];
            }
        }

        for (int ind = 0; ind < myActiveCells.Count; ind++)
        {
            myActiveCells[ind].MyInitializeNeighbourCells();
        }

        yield return new WaitForSeconds(0.1f);
        MyActiveCell.MyEventMouseClickOnCell += MyEventMouseClickOnCell;
        MyGem.MyEventEndPlayAnimationCell += MyEventEndPlayAnimationCell;
        MyEventStateSystem += MyMainEventStateSystem;
        myBufferTypeSpawn.indexDynamite = -1;
        MyEventStateSystem?.Invoke(MyGameState.MOVE);
    }

    private void MySpawnPower()
    {
        if(myBufferTypeSpawn.indexDynamite >= 0)
        {
            MyActiveCell currentActiveCell = myCells[myBufferTypeSpawn.indexDynamite].MyGetActiveCell;
            MyGem gem = Instantiate(Resources.Load<MyGem>("my_prefab/my_prefab_gem"));
            
            MyGemType gemType = Instantiate(Resources.Load<MyGemType>(myBufferTypeSpawn.typePrefab));
            
            if (currentActiveCell.transform.childCount == 0)
            {
                GameObject goPowerDynamite = Resources.Load<GameObject>("my_prefab/my_prefab_power_dynamite");
                MySpawnGemPower(ref currentActiveCell, gem, gemType, goPowerDynamite);
            }
            else
                print("error child.count = " + currentActiveCell.transform.childCount);
        }
        else if (myBufferTypeSpawn.quantity > 3)
        {
            MyActiveCell currentActiveCell = myCells[myBufferTypeSpawn.index].MyGetActiveCell;
            MyGem gem = Instantiate(Resources.Load<MyGem>("my_prefab/my_prefab_gem"));
            MyGemType gemType = Instantiate(Resources.Load<MyGemType>(myBufferTypeSpawn.typePrefab));

            if (currentActiveCell.transform.childCount == 0 && myBufferTypeSpawn.quantity == 4)
            {
                GameObject goPowerVectical = Resources.Load<GameObject>("my_prefab/my_prefab_power_four_v");
                GameObject goPowerHorizontal = Resources.Load<GameObject>("my_prefab/my_prefab_power_four_h");

                if(myBufferTypeSpawn.powerDirection == MyPowerDirection.HORIZONTAL)
                    MySpawnGemPower(ref currentActiveCell, gem, gemType, goPowerHorizontal);
                else
                    MySpawnGemPower(ref currentActiveCell, gem, gemType, goPowerVectical);
            }
            else if (currentActiveCell.transform.childCount == 0 && myBufferTypeSpawn.quantity == 5)
            {
                GameObject goPowerFive = Resources.Load<GameObject>("my_prefab/my_prefab_power_five");
                MySpawnGemPower(ref currentActiveCell, gem, gemType, goPowerFive);
            }
            else
                print("error child.count = " + currentActiveCell.transform.childCount);
        }
    }

    private void MySpawnGemPower(ref MyActiveCell _currentActiveCell, MyGem _gem, MyGemType _gemType,
        GameObject _prefabPower)
    {
        _gemType.transform.SetParent(_gem.transform, false);
        _gem.transform.SetParent(_currentActiveCell.transform, false);
        _gemType.myPathPrefab = myBufferTypeSpawn.typePrefab;
        _currentActiveCell.myIsTestBusy = true;

        GameObject goPower = Instantiate(_prefabPower);
        goPower.transform.SetParent(_gemType.transform, false);

        myBufferTypeSpawn.MyClear();
    }

    private void MyMainEventStateSystem(MyGameState _state)
    {
        if (myCoroutineGameState != null)
            StopCoroutine(myCoroutineGameState);
        myCoroutineGameState = StartCoroutine(MyStateGameChange(_state));
    }

    private IEnumerator MyStateGameChange(MyGameState _state)
    {
        MySpawnPower();
        
        yield return new WaitForSeconds(0.1f);

        switch (_state)
        {
            case MyGameState.MOVE:
                for (int index = 0; index < 20; index++)
                {
                    MySpawnGems();
                    MyCalculateMoveDownLeftOrRightToPoint();
                    MyMoveGems();
                    MySetParentCellNewCell();

                    yield return new WaitForSeconds(0.15f);

                    if (MyIsBusySpawners())
                    {
                        break;
                    }
                }
                List<MyActiveCell> cells = MySelectTheComboAndAddPower();

                myBufferTypeSpawn.indexDynamite = MyIsGetIndexDynamiteInComboArray(cells);
                if (myBufferTypeSpawn.indexDynamite >= 0)
                    myBufferTypeSpawn.typePrefab = cells[0].MyGetGem.MyGetType.myPathPrefab;
                
                if (cells.Count > 0)
                    MyDestroyGemsArray(cells.ToArray());

                yield return new WaitForSeconds(0.01f);

                //UnityEditor.EditorApplication.isPaused = true;

                if (cells.Count > 0)
                    MyEventStateSystem?.Invoke(MyGameState.MOVE);
                else
                    MyEventStateSystem?.Invoke(MyGameState.HANDLE);

                break;
            case MyGameState.HANDLE:
                myIsHandle = true;
                break;
            case MyGameState.WAIT:
                myIsHandle = false;
                yield return new WaitForSeconds(1);
                myIsHandle = true;
                MyDestroyGemsArray(myBufferDestroyPowerCell.myGetActiveCell.ToArray());
                MyMainEventStateSystem(MyGameState.MOVE);
                break;
            default:
                break;
        }
    }

    private void MyDestroyGemsArray(MyActiveCell[] _cells)
    {
        Stack<MyActiveCell> myActiveCellStack = new Stack<MyActiveCell>(_cells);
        //print("start stack count = " + myActiveCellStack.Count);
        for (int ind = 0; myActiveCellStack.Count > 0 && ind < myWidth * myHeight; ind++)
        {
            //print("stack pop and push count = " + myActiveCellStack.Count);
            MyActiveCell activeCell = myActiveCellStack.Pop();
            MyActiveCell[] arrActiveCell = activeCell.MyDestroyGem(this);

            if (arrActiveCell != null && arrActiveCell.Length > 0)
            {
                //print("power activate arr count = " + arrActiveCell.Length);
                for (int arrInd = 0; arrInd < arrActiveCell.Length; arrInd++)
                {
                    myActiveCellStack.Push(arrActiveCell[arrInd]);
                }
            }
        }
    }

    private List<MyActiveCell> MySelectTheComboAndAddPower()
    {
        List<MyActiveCell> arrList = new List<MyActiveCell>();
        for (int ind = 0; ind < myWidth; ind++)
        {
            arrList.AddRange(MyIsLookACombo(ind, ind));
        }

        for (int ind = 0; ind < arrList.Count; ind++)
        {
            arrList[ind].MyGetGem.myIsAddedListForDelete = true;
        }

        return arrList;
    }

    private bool MyIsBusySpawners()
    {
        for (int ind = 0; ind < mySpawnerGem.Count; ind++)
        {
            if (!mySpawnerGem[ind].MyIsHaveGem)
                return false;
        }

        return true;
    }

    private void MySpawnGems()
    {
        for (int ind = 0; ind < mySpawnerGem.Count; ind++)
        {
            mySpawnerGem[ind].MySpawnGem();
        }
    }

    private void MyMoveGems()
    {
        for (int ind = 0; ind < myActiveCells.Count; ind++)
        {
            if(myActiveCells[ind].MyGetGem && !myActiveCells[ind].MyGetGem.MyIsAnimate &&
                myActiveCells[ind].MyGetGem.myListPoints.Count > 0)
            {
                myActiveCells[ind].MyGetGem.MyMoveGem();
            }
        }
    }

    private void MyAddLineGemsCombo(MyActiveCell _first, MyActiveCell _second,
        ref List<MyActiveCell> _comboList, ref List<MyActiveCell> _lineList)
    {
        if (_first != null && _second != null)
        {
            if (_first.MyGetGem && _second.MyGetGem)
            {
                if (_lineList.Count == 0 && (_first.MyGetGem.MyGetType.myType == _second.MyGetGem.MyGetType.myType))
                {
                    _lineList.Add(_first);
                    _lineList.Add(_second);
                }
                else if (_lineList.Count > 0 && (_first.MyGetGem.MyGetType.myType == _second.MyGetGem.MyGetType.myType))
                {
                    _lineList.Add(_second);
                }
                else if (_lineList.Count > 2 && (_first.MyGetGem.MyGetType.myType != _second.MyGetGem.MyGetType.myType))
                {
                    _comboList.AddRange(_lineList);
                    _lineList.Clear();
                }
                else
                    _lineList.Clear();
            }
            else
            {
                if (_lineList.Count > 2)
                {
                    _comboList.AddRange(_lineList);
                    _lineList.Clear();
                }
                else
                    _lineList.Clear();
            }
        }
        else
        {
            if (_lineList.Count > 2)
            {
                _comboList.AddRange(_lineList);
                _lineList.Clear();
            }
            else
                _lineList.Clear();
        }
    }

    private MyActiveCell[] MyIsThereACombo(int _x, int _y)
    {
        List<MyActiveCell> comboList = new List<MyActiveCell>();
        List<MyActiveCell> tempLineList = new List<MyActiveCell>();
        
        for (int y = 0; y < myCells2D.GetLength(1) - 1; y++)
        {
            MyActiveCell activeCellFirst = myCells2D[_x, y].MyGetActiveCell;
            MyActiveCell activeCellSecond = myCells2D[_x, y + 1].MyGetActiveCell;

            MyAddLineGemsCombo(activeCellFirst, activeCellSecond, ref comboList, ref tempLineList);
            
            if (y + 1 == myCells2D.GetLength(1) - 1 && tempLineList.Count > 2)
            {
                comboList.AddRange(tempLineList);
                tempLineList.Clear();
            }
        }

        for (int x = 0; x < myCells2D.GetLength(0) - 1; x++)
        {
            MyActiveCell activeCellFirst = myCells2D[x, _y].MyGetActiveCell;
            MyActiveCell activeCellSecond = myCells2D[x + 1, _y].MyGetActiveCell;

            MyAddLineGemsCombo(activeCellFirst, activeCellSecond, ref comboList, ref tempLineList);
            
            if (x + 1 == myCells2D.GetLength(0) - 1 && tempLineList.Count > 2)
            {
                comboList.AddRange(tempLineList);
                tempLineList.Clear();
            }
        }
        return comboList.ToArray();
    }

    private MyActiveCell[] MySeeAllCombos()
    {
        List<MyActiveCell> arrList = new List<MyActiveCell>();
        for (int ind = 0; ind < myWidth; ind++)
        {
            arrList.AddRange(MyIsThereACombo(ind, ind));
        }
        
        return arrList.ToArray();
    }

    private void MyLookLineGemsCombo(MyActiveCell _first, MyActiveCell _second,
       ref List<MyActiveCell> _comboList, ref List<MyActiveCell> _lineList, MyPowerDirection _powerDir)
    {
        if (_first != null && _second != null)
        {
            if (_first.MyGetGem && _second.MyGetGem)
            {
                if (_lineList.Count == 0 && (_first.MyGetGem.MyGetType.myType == _second.MyGetGem.MyGetType.myType))
                {
                    _lineList.Add(_first);
                    _lineList.Add(_second);
                }
                else if (_lineList.Count > 0 && (_first.MyGetGem.MyGetType.myType == _second.MyGetGem.MyGetType.myType))
                {
                    _lineList.Add(_second);
                }
                else if (_lineList.Count > 2 && (_first.MyGetGem.MyGetType.myType != _second.MyGetGem.MyGetType.myType))
                {
                    if (_lineList.Count == 5)
                    {
                        myBufferTypeSpawn.typePrefab = _lineList[1].MyGetGem.MyGetType.myPathPrefab;
                        myBufferTypeSpawn.quantity = 5;
                        myBufferTypeSpawn.index = _lineList[1].MyGetId();
                        myBufferTypeSpawn.powerDirection = _powerDir;
                    }
                    else if (_lineList.Count == 4)
                    {
                        myBufferTypeSpawn.typePrefab = _lineList[1].MyGetGem.MyGetType.myPathPrefab;
                        myBufferTypeSpawn.quantity = 4;
                        myBufferTypeSpawn.index = _lineList[1].MyGetId();
                        myBufferTypeSpawn.powerDirection = _powerDir;
                    }

                    _comboList.AddRange(_lineList);
                    _lineList.Clear();
                }
                else
                    _lineList.Clear();
            }
            else
            {
                if (_lineList.Count > 2)
                {
                    if (_lineList.Count == 5)
                    {
                        myBufferTypeSpawn.typePrefab = _lineList[1].MyGetGem.MyGetType.myPathPrefab;
                        myBufferTypeSpawn.quantity = 5;
                        myBufferTypeSpawn.index = _lineList[1].MyGetId();
                        myBufferTypeSpawn.powerDirection = _powerDir;
                    }
                    else if (_lineList.Count == 4)
                    {
                        myBufferTypeSpawn.typePrefab = _lineList[1].MyGetGem.MyGetType.myPathPrefab;
                        myBufferTypeSpawn.quantity = 4;
                        myBufferTypeSpawn.index = _lineList[1].MyGetId();
                        myBufferTypeSpawn.powerDirection = _powerDir;
                    }

                    _comboList.AddRange(_lineList);
                    _lineList.Clear();
                }
                else
                    _lineList.Clear();
            }
        }
        else
        {
            if (_lineList.Count > 2)
            {
                if (_lineList.Count == 5)
                {
                    myBufferTypeSpawn.typePrefab = _lineList[1].MyGetGem.MyGetType.myPathPrefab;
                    myBufferTypeSpawn.quantity = 5;
                    myBufferTypeSpawn.index = _lineList[1].MyGetId();
                    myBufferTypeSpawn.powerDirection = _powerDir;
                }
                else if (_lineList.Count == 4)
                {
                    myBufferTypeSpawn.typePrefab = _lineList[1].MyGetGem.MyGetType.myPathPrefab;
                    myBufferTypeSpawn.quantity = 4;
                    myBufferTypeSpawn.index = _lineList[1].MyGetId();
                    myBufferTypeSpawn.powerDirection = _powerDir;
                }

                _comboList.AddRange(_lineList);
                _lineList.Clear();
            }
            else
                _lineList.Clear();
        }
    }

    private MyActiveCell[] MyIsLookACombo(int _x, int _y)
    {
        List<MyActiveCell> comboList = new List<MyActiveCell>();
        List<MyActiveCell> tempLineList = new List<MyActiveCell>();

        for (int y = 0; y < myCells2D.GetLength(1) - 1; y++)
        {
            MyActiveCell activeCellFirst = myCells2D[_x, y].MyGetActiveCell;
            MyActiveCell activeCellSecond = myCells2D[_x, y + 1].MyGetActiveCell;

            MyLookLineGemsCombo(activeCellFirst, activeCellSecond, ref comboList, ref tempLineList,
                MyPowerDirection.VERTICAL);

            if (y + 1 == myCells2D.GetLength(1) - 1 && tempLineList.Count > 2)
            {
                if (tempLineList.Count == 5)
                {
                    myBufferTypeSpawn.typePrefab = tempLineList[1].MyGetGem.MyGetType.myPathPrefab;
                    myBufferTypeSpawn.quantity = 5;
                    myBufferTypeSpawn.index = tempLineList[1].MyGetId();
                    myBufferTypeSpawn.powerDirection = MyPowerDirection.VERTICAL;
                }
                else if (tempLineList.Count == 4)
                {
                    myBufferTypeSpawn.typePrefab = tempLineList[1].MyGetGem.MyGetType.myPathPrefab;
                    myBufferTypeSpawn.quantity = 4;
                    myBufferTypeSpawn.index = tempLineList[1].MyGetId();
                    myBufferTypeSpawn.powerDirection = MyPowerDirection.VERTICAL;
                }

                comboList.AddRange(tempLineList);
                tempLineList.Clear();
            }
        }

        for (int x = 0; x < myCells2D.GetLength(0) - 1; x++)
        {
            MyActiveCell activeCellFirst = myCells2D[x, _y].MyGetActiveCell;
            MyActiveCell activeCellSecond = myCells2D[x + 1, _y].MyGetActiveCell;

            MyLookLineGemsCombo(activeCellFirst, activeCellSecond, ref comboList, ref tempLineList,
                MyPowerDirection.HORIZONTAL);

            if (x + 1 == myCells2D.GetLength(0) - 1 && tempLineList.Count > 2)
            {
                if (tempLineList.Count == 5)
                {
                    myBufferTypeSpawn.typePrefab = tempLineList[1].MyGetGem.MyGetType.myPathPrefab;
                    myBufferTypeSpawn.quantity = 5;
                    myBufferTypeSpawn.index = tempLineList[1].MyGetId();
                    myBufferTypeSpawn.powerDirection = MyPowerDirection.HORIZONTAL;
                }
                else if (tempLineList.Count == 4)
                {
                    myBufferTypeSpawn.typePrefab = tempLineList[1].MyGetGem.MyGetType.myPathPrefab;
                    myBufferTypeSpawn.quantity = 4;
                    myBufferTypeSpawn.index = tempLineList[1].MyGetId();
                    myBufferTypeSpawn.powerDirection = MyPowerDirection.HORIZONTAL;
                }

                comboList.AddRange(tempLineList);
                tempLineList.Clear();
            }
        }

        return comboList.ToArray();
    }

    private int MyIsGetIndexDynamiteInComboArray(List<MyActiveCell> _arrCombo)
    {
        foreach (var main in _arrCombo)
        {
            int currId = main.MyGetId();
            int count = 0;

            foreach (var test in _arrCombo)
            {
                int nextId = test.MyGetId();

                if (currId == nextId)
                    count++;

                if (count > 1)
                    return currId;
            }
        }

        return -1;
    }

    private void MySetParentCellNewCell()
    {
        for (int ind = 0; ind < myActiveCells.Count; ind++)
        {
            if(myActiveCells[ind].MyIsNeedToMoveTransformParent)
            {
                if (myActiveCells[ind].MyGetGem)
                {
                    int lastIndex = myActiveCells[ind].MyGetGem.myListPoints.Count - 1;

                    myActiveCells[ind].MyGetGem.transform.SetParent(
                        myActiveCells[ind].MyGetGem.myListPoints[lastIndex].transform, true);

                    myActiveCells[ind].MyIsNeedToMoveTransformParent = false;
                }
                else
                {
                    print("wrong gem is null cell = " + myActiveCells[ind].transform.parent.name);
                }
            }
        }
    }

    private void MyCheckMoveOnGems()
    {
        List<MyStateCell> listStateCell = new List<MyStateCell>();
        List<bool> listStateBool = new List<bool>();

        for (int ind = 0; ind < myCells.Count; ind++)
        {
            if (ind < myWidth && myCells[ind].MyGetActiveCell && 
                myCells[ind].MyGetActiveCell.GetComponent<MySpawnerGem>())
                listStateBool.Add(true);
            else
                listStateBool.Add(false);

            if (!myCells[ind].MyGetActiveCell)
                listStateCell.Add(MyStateCell.EMPTY);
            else if (myCells[ind].MyGetActiveCell && myCells[ind].MyGetActiveCell.GetComponent<MySpawnerGem>())
                listStateCell.Add(MyStateCell.RESPAWN);
            else
                listStateCell.Add(MyStateCell.ACTIVE);
        }

        for (int tact = 0; tact < myHeight; tact++)
        {
            for (int ind = 0; ind < myCells.Count; ind++)
            {
                if (listStateCell[ind] == MyStateCell.RESPAWN)
                {
                    int nextIndex = -1;
                    int pushIndex = 0;

                    for (int y = 0; y < myHeight + 1; y++)
                    {
                        nextIndex = (y * myWidth + ind) + pushIndex;

                        if (MyIsCellActiveInRange(listStateCell.ToArray(), nextIndex))
                        {
                            listStateBool[nextIndex] = true;
                        }
                        else if (MyIsCellActiveInRange(listStateCell.ToArray(), nextIndex - 1))
                        {
                            pushIndex--;
                            nextIndex = (y * myWidth + ind) + pushIndex;
                            listStateBool[nextIndex] = true;
                        }
                        else if (MyIsCellActiveInRange(listStateCell.ToArray(), nextIndex + 1))
                        {
                            pushIndex++;
                            nextIndex = (y * myWidth + ind) + pushIndex;
                            listStateBool[nextIndex] = true;
                        }
                        else
                        {
                            if (MyIsCellActiveInRange(nextIndex) && (listStateCell[nextIndex] == MyStateCell.EMPTY ||
                                listStateCell[nextIndex] == MyStateCell.BUSY))
                                break;
                        }
                    }

                    if (nextIndex > 9)
                        listStateCell[nextIndex - myWidth] = MyStateCell.BUSY;
                }
            }
        }

        MyTestActiveCellColor(listStateBool.ToArray());
    }

    private bool MyIsCellActiveInRange(MyStateCell[] _cells, int _index)
    {
        try
        {
            if (_cells[_index] == MyStateCell.ACTIVE)
                return true;
        }
        catch(System.Exception ex) { }

        return false;
    }

    private bool MyIsCellActiveInRange(int _index)
    {
        try
        {
            if (myCells[_index])
                return true;
        }
        catch (System.Exception ex) { }

        return false;
    }

    private void MyCalculateMoveDownLeftOrRightToPoint()
    {
        MyCheckMoveOnGems();

        for (int ind = myActiveCells.Count - 1; ind >= 0; ind--)
        {
            if(myActiveCells[ind].MyGetGem && !myActiveCells[ind].MyGetGem.MyIsAnimate)
            {
                MyActiveCell currentActiveCell = myActiveCells[ind];
                MyActiveCell parentActiveCell = myActiveCells[ind];

                parentActiveCell.MyGetGem.myListPoints.Clear();

                for (int indY = 0; indY < myHeight; indY++)
                {
                    bool isSideLeft = currentActiveCell.MyIsCheckSidesLeft();
                    bool isSideRight = currentActiveCell.MyIsCheckSidesRight();

                    bool isDown = currentActiveCell.MyNextCellDown(MyDirectionForCell.MIDDLE);
                    bool isLeft = currentActiveCell.MyNextCellDown(MyDirectionForCell.LEFT);
                    bool isRight = currentActiveCell.MyNextCellDown(MyDirectionForCell.RIGHT);

                    if (isSideLeft || isSideRight)
                    {
                        if (isDown && currentActiveCell.MyNextCellDown(MyDirectionForCell.MIDDLE,
                            out currentActiveCell))
                        {
                            parentActiveCell.MyGetGem.myListPoints.Add(currentActiveCell);
                        }
                        else if (isLeft && currentActiveCell.MyIsHaveTopCell() && 
                            (!currentActiveCell.MyGetCellTop() || 
                            (currentActiveCell.MyGetCellTop() && !currentActiveCell.MyGetCellTop().myIsMove)) &&
                            currentActiveCell.MyNextCellDown(MyDirectionForCell.LEFT, out currentActiveCell))
                        {
                            parentActiveCell.MyGetGem.myListPoints.Add(currentActiveCell);
                        }
                        else if (isRight && currentActiveCell.MyIsHaveTopCell() &&
                            (!currentActiveCell.MyGetCellTop() ||
                            (currentActiveCell.MyGetCellTop() && !currentActiveCell.MyGetCellTop().myIsMove)) &&
                            currentActiveCell.MyNextCellDown(MyDirectionForCell.RIGHT, out currentActiveCell))
                        {
                            parentActiveCell.MyGetGem.myListPoints.Add(currentActiveCell);
                        }
                    }
                    else
                    {
                        if (isDown)
                        {
                            currentActiveCell.MyNextCellDown(MyDirectionForCell.MIDDLE, out currentActiveCell);
                            parentActiveCell.MyGetGem.myListPoints.Add(currentActiveCell);
                        }
                        else if (isLeft && !currentActiveCell.MyGetActiveCellByIndex(0))
                        {
                            currentActiveCell.MyNextCellDown(MyDirectionForCell.LEFT, out currentActiveCell);
                            parentActiveCell.MyGetGem.myListPoints.Add(currentActiveCell);
                        }
                        else if (isRight && !currentActiveCell.MyGetActiveCellByIndex(1))
                        {
                            currentActiveCell.MyNextCellDown(MyDirectionForCell.RIGHT, out currentActiveCell);
                            parentActiveCell.MyGetGem.myListPoints.Add(currentActiveCell);
                        }
                    }
                }

                if (parentActiveCell.MyGetGem.myListPoints.Count > 0)
                {
                    parentActiveCell.MyIsNeedToMoveTransformParent = true;
                    parentActiveCell.myIsTestBusy = false;
                    int lastIndex = parentActiveCell.MyGetGem.myListPoints.Count - 1;
                    parentActiveCell.MyGetGem.myListPoints[lastIndex].myIsTestBusy = true;
                }
            }
        }
    }

    private void MyTestActiveCellColor(bool[] _arrBusy)
    {
        for (int ind = 0; ind < _arrBusy.Length; ind++)
        {
            if (_arrBusy[ind])
                myCells[ind].MyGetActiveCell.MySetColor(Color.red);
        }
    }

    private void MyCellOnExchange(MyActiveCell _first, MyActiveCell _target)
    {
        if(_target.MyGetGem && myIsHandle && 
            _first.MyGetGem.GetComponentInChildren<MyPowerGem>() &&
            _first.MyGetGem.GetComponentInChildren<MyPowerGem>().myPower == MyTypePower.FIVE)
        {
            List<MyActiveCell> cells = MyCollectAllTypeGems(_first, _target);
            GameObject goPrefabLineDraw = Resources.Load<GameObject>("my_prefab/my_effect/myEffectLinesDraw");
            GameObject goInstanceLineDraw = Instantiate(goPrefabLineDraw);
            goInstanceLineDraw.transform.SetParent(_first.GetComponentInChildren<MyPowerGem>().transform);
            MyDrawLines drawLines = goInstanceLineDraw.GetComponent<MyDrawLines>();
            drawLines.MyInitialize(cells.ToArray());

            myBufferDestroyPowerCell.MyClearBuffer();
            myBufferDestroyPowerCell.MySetArrayActiveCell(cells);

            MyMainEventStateSystem(MyGameState.WAIT);
        }
        else if (_target.MyGetGem && myIsHandle)
        {
            _first.MyGetGem.MyMoveToPointTheCell(_target);
            _target.MyGetGem.MyMoveToPointTheCell(_first);

            myIsOneCallCell = true;
            myIsHandle = false;
        }
    }

    private List<MyActiveCell> MyCollectAllTypeGems(MyActiveCell _first, MyActiveCell _targetCell)
    {
        List<MyActiveCell> listCells = new List<MyActiveCell>();

        for (int ind = 0; ind < myActiveCells.Count; ind++)
        {
            if (myActiveCells[ind].MyGetGem &&
                myActiveCells[ind].GetComponentInChildren<MyGemType>().myType ==
                _targetCell.GetComponentInChildren<MyGemType>().myType)
            {
                listCells.Add(myActiveCells[ind]);
            }
        }

        listCells.Add(_first);

        return listCells;
    }

    private MyPairInt MyGetIndexCell(MyActiveCell _cell)
    {
        MyPairInt pair;
        pair.x = -1;
        pair.y = -1;

        for (int y = 0; y < myCells2D.GetLength(1); y++)
        {
            for (int x = 0; x < myCells2D.GetLength(0); x++)
            {
                if (_cell.MyGetId() == myCells2D[x, y].MyGetId())
                {
                    pair.x = x;
                    pair.y = y;
                    return pair;
                }
            }
        }
        return pair;
    }

    private void MyEventMouseClickOnCell(MyActiveCell _cell, MyDirectionMouse _dir)
    {
        if (myIsHandle)
        {
            MyPairInt index;
            MyActiveCell cellTarget = null;

            try
            {
                switch (_dir)
                {
                    case MyDirectionMouse.UNKNOWN:
                        break;
                    case MyDirectionMouse.LEFT:
                        index = MyGetIndexCell(_cell);
                        cellTarget = myCells2D[index.x - 1, index.y].MyGetActiveCell;
                        myBufferPairActiveCell.first = _cell;
                        myBufferPairActiveCell.second = cellTarget;
                        if (cellTarget)
                            MyCellOnExchange(_cell, cellTarget);
                        break;
                    case MyDirectionMouse.RIGHT:
                        index = MyGetIndexCell(_cell);
                        cellTarget = myCells2D[index.x + 1, index.y].MyGetActiveCell;
                        myBufferPairActiveCell.first = _cell;
                        myBufferPairActiveCell.second = cellTarget;
                        if (cellTarget)
                            MyCellOnExchange(_cell, cellTarget);
                        break;
                    case MyDirectionMouse.UP:
                        index = MyGetIndexCell(_cell);
                        cellTarget = myCells2D[index.x, index.y - 1].MyGetActiveCell;
                        myBufferPairActiveCell.first = _cell;
                        myBufferPairActiveCell.second = cellTarget;
                        if (cellTarget)
                            MyCellOnExchange(_cell, cellTarget);
                        break;
                    case MyDirectionMouse.DOWN:
                        index = MyGetIndexCell(_cell);
                        cellTarget = myCells2D[index.x, index.y + 1].MyGetActiveCell;
                        myBufferPairActiveCell.first = _cell;
                        myBufferPairActiveCell.second = cellTarget;
                        if (cellTarget)
                            MyCellOnExchange(_cell, cellTarget);
                        break;
                    default:
                        print("Unknow command direction");
                        break;
                }
            }
            catch (System.Exception _ex) { }
        }
    }

    private void MyEventEndPlayAnimationCell()
    {
        if (myIsOneCallCell)
        {
            myIsOneCallCell = false;
            
            if (MySeeAllCombos().Length == 0)
            {
                myBufferPairActiveCell.second.MyGetGem.MyMoveToPointTheCell(myBufferPairActiveCell.first);
                myBufferPairActiveCell.first.MyGetGem.MyMoveToPointTheCell(myBufferPairActiveCell.second);
                myIsHandle = true;
            }
            else
            {
                myIsHandle = false;
                MyMainEventStateSystem(MyGameState.MOVE);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(0);
    }
}
