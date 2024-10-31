using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MyDirectionForCell { LEFT, MIDDLE, RIGHT }

public class MyActiveCell : MonoBehaviour, MyMisc.MyISortObjectId, IPointerDownHandler, IPointerUpHandler
{
    private int myId;

    private GameObject myParticleDestroy;
    private GameObject myPrefabParticleEffectLineHorizont;
    private GameObject myPrefabParticleEffectLineVertical;
    private GameObject myPrefabBombExplode;
    private MyCell myCellTop;
    private MyActiveCell myCellActiveTop;
    private MyCell[] myNeighbourCells;
    private MyActiveCell[] mySides;

    private Vector2 myOldPositionMouse;
    private Vector2 myPositionMouse;

    private const float myTimeStop = 40f;
    private static float myTimeCurrent = 0;

    public bool myIsTestBusy;
    public bool myIsMove;

    public bool MyIsNeedToMoveTransformParent = false;

    public MyGem MyGetGem { get { return transform.childCount > 0 ? 
                transform.GetComponentInChildren<MyGem>() : null; } }

    public static event System.Action<MyActiveCell, MyDirectionMouse> MyEventMouseClickOnCell;

    private void Awake()
    {
        myParticleDestroy = Resources.Load("my_prefab/my_prefab_particle_destroy") as GameObject;
        myPrefabParticleEffectLineHorizont = Resources.Load("my_prefab/my_effect/myEffectLineHorizont") as GameObject;
        myPrefabParticleEffectLineVertical = Resources.Load("my_prefab/my_effect/myEffectLineVertical") as GameObject;
        myPrefabBombExplode = Resources.Load("my_prefab/my_effect/myEffectBombExplode") as GameObject;
    }

    public int MyGetId()
    {
        myId = MyMisc.MyStringToInt(transform.parent.name);
        return myId;
    }

    public void MySetColor(Color _color)
    {
        //Image img = GetComponent<Image>();
        //img.color = _color;

        if (_color == Color.red)
            myIsMove = true;
        else
            myIsMove = false;
    }

    public void MyInitializeNeighbourCells()
    {
        myIsMove = false;

        if (MyGetGem)
            myIsTestBusy = true;
        else
            myIsTestBusy = false;

        Vector3 offset = new Vector3(0, -100, 0);
        Collider2D[] collidersMove = Physics2D.OverlapBoxAll(transform.position + offset, new Vector2(300, 100), 0);
        myNeighbourCells = new MyCell[3] { null, null, null };
        mySides = new MyActiveCell[2] { null, null };

        Collider2D collidersSideLeft = Physics2D.OverlapBox(transform.position + new Vector3(-100, 0, 0),
            new Vector2(100, 100), 0);
        Collider2D collidersSideRight = Physics2D.OverlapBox(transform.position + new Vector3(100, 0, 0),
            new Vector2(100, 100), 0);
        mySides[0] = collidersSideLeft.GetComponentInChildren<MyActiveCell>(); 
        mySides[1] = collidersSideRight.GetComponentInChildren<MyActiveCell>();

        Collider2D colliderTop = Physics2D.OverlapBox(transform.position + new Vector3(0, 100, 0),
            new Vector3(100, 100), 0);
        if (colliderTop)
            myCellTop = colliderTop.GetComponentInChildren<MyCell>();
        else
            myCellTop = null;
        myCellActiveTop = myCellTop ? myCellTop.MyGetActiveCell : null;

        for (int index = 0; index < collidersMove.Length; index++)
            if(collidersMove[index])
                myNeighbourCells[index] = collidersMove[index].GetComponent<MyCell>();
        
        if (collidersMove.Length > 0)
            Array.Sort(myNeighbourCells, new MyMisc.MySortObjectId());
    }

    public bool MyIsCheckSidesLeft()
    {
        return mySides[0] && !mySides[0].myIsTestBusy;
    }
    public bool MyIsCheckSidesRight()
    {
        return mySides[1] && !mySides[1].myIsTestBusy;
    }
    public MyActiveCell MyGetActiveCellByIndex(int _index)
    {
        return mySides[_index] ? mySides[_index] : null;
    }
    public MyActiveCell MyGetCellTop()
    {
        return myCellActiveTop;
    }
    public bool MyIsHaveTopCell()
    {
        return myCellTop != null;
    }

    //private void MyRecursionCollectPowersCell(int _index, List<MyActiveCell> _listPowers, 
    //    List<MyActiveCell> _listGems, MyMain _main)
    //{
    //    if(_index < _listPowers.Count)
    //    {
    //        MyPowerGem power = _listPowers[_index].GetComponentInChildren<MyPowerGem>();

    //        _listGems.AddRange(MyGetTheLineGemForDestroy(_main, _listPowers[_index].MyGetId(), power.myPower));
    //        _listPowers.AddRange(MyGetThePowersOfTheGem(_listGems));

    //        MyRecursionCollectPowersCell(++_index, _listPowers, _listGems, _main);
    //    }
    //}

    private MyActiveCell[] MyDetectPowerGems(MyMain _main, MyPowerGem _power)
    {
        if (_power.myPower == MyTypePower.FOUR_HORIZONTAL)
        {
            GameObject goEffect = Instantiate(myPrefabParticleEffectLineHorizont, 
                transform.position + new Vector3(0, 0, -1), Quaternion.identity);
            Destroy(goEffect, 0.5f);

            return MyGetTheLineGemForDestroy(_main, MyGetId(), _power.myPower);
        }
        else if (_power.myPower == MyTypePower.FOUR_VERTICAL)
        {
            GameObject goEffect = Instantiate(myPrefabParticleEffectLineVertical,
                transform.position + new Vector3(0, 0, -1), Quaternion.identity);
            Destroy(goEffect, 0.5f);

            return MyGetTheLineGemForDestroy(_main, MyGetId(), _power.myPower);
        }
        else if(_power.myPower == MyTypePower.BOMB3X3)
        {
            GameObject goEffect = Instantiate(myPrefabBombExplode, transform.position + new Vector3(0, 0, -1),
                Quaternion.Euler(new Vector3(-180, 0, 0)));
            goEffect.GetComponent<MyBombExplodeScript>().MyStartAnimation(0.01f);
            
            Destroy(goEffect, 0.5f);

            return MyCollectCellAfterExplodeDynamite().ToArray();
        }
        else
        {
            print("exeption in MyDetectPowerGems uknown power");
            return null;
        }
    }

    private List<MyActiveCell> MyCollectCellAfterExplodeDynamite()
    {
        Collider2D[] arrCells = Physics2D.OverlapBoxAll(transform.position, new Vector2(300, 300), 0);
        List<MyActiveCell> destroyDynamitActiveCell = new List<MyActiveCell>();

        for (int ind = 0; ind < arrCells.Length; ind++)
        {
            if (arrCells[ind].GetComponentInChildren<MyActiveCell>() &&
                arrCells[ind].GetComponentInChildren<MyActiveCell>().MyGetGem &&
                !arrCells[ind].GetComponentInChildren<MyActiveCell>().MyGetGem.myIsAddedListForDelete)
                destroyDynamitActiveCell.Add(arrCells[ind].GetComponentInChildren<MyActiveCell>());
        }

        return destroyDynamitActiveCell;
    }

    private MyActiveCell[] MyGetTheLineGemForDestroy(MyMain _main, int _index, MyTypePower _typePower)
    {
        List<MyActiveCell> _list = new List<MyActiveCell>();

        int x = (_index % _main.myWidth);
        int y = (_index / _main.myHeight);

        if (_typePower == MyTypePower.FOUR_HORIZONTAL)
        {
            for (int lineX = 0; lineX < _main.myWidth; lineX++)
            {
                if (_main.MyGetCells2D[lineX, y].MyGetActiveCell &&
                    _main.MyGetCells2D[lineX, y].MyGetActiveCell.MyGetGem &&
                    !_main.MyGetCells2D[lineX, y].MyGetActiveCell.MyGetGem.myIsAddedListForDelete)
                {
                    if (!_main.MyGetCells2D[lineX, y].MyGetActiveCell.MyGetGem.GetComponentInChildren<MyPowerGem>())
                        _main.MyGetCells2D[lineX, y].MyGetActiveCell.MyGetGem.myIsAddedListForDelete = true;
                    _list.Add(_main.MyGetCells2D[lineX, y].MyGetActiveCell);
                }
            }
        }
        else if(_typePower == MyTypePower.FOUR_VERTICAL)
        {
            for (int lineY = 0; lineY < _main.myWidth; lineY++)
            {
                if (_main.MyGetCells2D[x, lineY].MyGetActiveCell &&
                    _main.MyGetCells2D[x, lineY].MyGetActiveCell.MyGetGem &&
                    !_main.MyGetCells2D[x, lineY].MyGetActiveCell.MyGetGem.myIsAddedListForDelete)
                {
                    if (!_main.MyGetCells2D[x, lineY].MyGetActiveCell.MyGetGem.GetComponentInChildren<MyPowerGem>())
                        _main.MyGetCells2D[x, lineY].MyGetActiveCell.MyGetGem.myIsAddedListForDelete = true;
                    _list.Add(_main.MyGetCells2D[x, lineY].MyGetActiveCell);
                }
            }
        }

        return _list.ToArray();
    }

    public MyActiveCell[] MyDestroyGem(MyMain _main)
    {
        if (MyGetGem)
        { 
            MyPowerGem power = GetComponentInChildren<MyPowerGem>();
            if (power)
            {
                MyGetGem.myIsAddedListForDelete = true;
                MyDestructGem();

                return MyDetectPowerGems(_main, power);
            }

            MyDestructGem();
        }
        return null;
    }

    private void MyDestructGem()
    {
        myIsTestBusy = false;
        myIsMove = false;

        GameObject goParticle = Instantiate(myParticleDestroy, transform.position, Quaternion.identity);
        goParticle.transform.SetParent(transform.parent, true);
        Destroy(goParticle, 1);
        Destroy(MyGetGem.gameObject);
    }

    public bool MyNextCellDown(MyDirectionForCell _dirCell, out MyActiveCell _activeCell)
    {
        MyActiveCell activeCell = MyGetDirectionNeighbourCellActive(_dirCell);
        _activeCell = activeCell;

        if (activeCell && !activeCell.myIsTestBusy)
            return true;
        else
            return false;
    }

    public bool MyNextCellDown(MyDirectionForCell _dirCell)
    {
        MyActiveCell activeCell = MyGetDirectionNeighbourCellActive(_dirCell);

        if (activeCell && !activeCell.myIsTestBusy)
            return true;
        else
            return false;
    }

    public bool MyHaveNextCell(MyDirectionForCell _dirCell)
    {
        MyActiveCell activeCell = MyGetDirectionNeighbourCellActive(_dirCell);

        if (activeCell)
            return true;
        else
            return false;
    }

    private MyActiveCell MyGetDirectionNeighbourCellActive(MyDirectionForCell _dirCell)
    {
        return myNeighbourCells[(int)_dirCell] != null ? myNeighbourCells[(int)_dirCell].MyGetActiveCell : null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        myPositionMouse = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (MyGetGem && (myTimeCurrent > myTimeStop))
        {
            myTimeCurrent = 0;
            myOldPositionMouse = eventData.position;

            Vector2 mouseDir = (myPositionMouse - myOldPositionMouse).normalized;
            MyDirectionMouse dir = MyDirectionMouse.UNKNOWN;

            if (mouseDir.x > 0.9f)
                dir = MyDirectionMouse.LEFT;
            else if (mouseDir.x < -0.9f)
                dir = MyDirectionMouse.RIGHT;
            else if (mouseDir.y > 0.9f)
                dir = MyDirectionMouse.DOWN;
            else if (mouseDir.y < -0.9f)
                dir = MyDirectionMouse.UP;

            MyEventMouseClickOnCell?.Invoke(this, dir);
        }
    }

    private void FixedUpdate()
    {
        myTimeCurrent += Time.deltaTime;
    }
}
