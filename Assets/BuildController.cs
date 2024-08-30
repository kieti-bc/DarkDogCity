using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum BuildingType : int
{
    RedHouse = 0,
    BlueHouse = 1
}

public class BuildController : MonoBehaviour
{
    public List<GameObject> housePrefabs;
    public Grid cityGrid; // This is the grid that is parent to all tilemaps
    public Tilemap baseMap; // This is the city tilemap with streets
    public float GridSizePixels;
    public float GridSizeUnits;

    public event Action<int> HousePlacedEvent;

    float gridX;
    float gridY;
    //Vector3 mouseWorld;
    Vector2Int wtum;


    private int MapWidth;
    private int MapHeight;
    int BuildingWidth = 3;
    int BuildingHeight = 4;


    public List<Sprite> canBuildSprites;

    // DEBUG
    bool hitBuilding = false;
    bool hitBorder = false;
    bool hitRoad = false;
    Vector2Int firstRoadHit = Vector2Int.zero;

    string roadCheck = "";

    private int selectedBuildingIndex = -1;

    int[,] tileContents;

    public GameObject activeCreation;
    // Start is called before the first frame update
    void Start()
    {
        // One unit is 8 pixels
        uint pw;
        uint ph;
        PlayModeWindow.GetRenderingResolution(out pw, out ph);
        int sw = Screen.width;
        int sh = Screen.height;
        // these are all the same
        // Debug.Log($"Resolutions: Camera ({gameWidth},{gameHeight}), PlayMode({pw}, {ph}) Screen ({sw},{sh})");

        GridSizeUnits = 1.0f;

        MapWidth = baseMap.size.x;
        MapHeight = baseMap.size.y;
        // Keep track where buildings have already been placed
        tileContents = new int[baseMap.size.x,baseMap.size.y] ;
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y= 0; y < MapHeight; y++)
            {
                tileContents[x, y] = 0;
            }
        }

        // Debug info about baseMap
        // Debug.Log($"BaseMap: Anchor: {baseMap.tileAnchor}, Tile 0x0 at {baseMap.GetCellCenterWorld(new Vector3Int(0, 0))} Tile wxh at {baseMap.GetCellCenterWorld(new Vector3Int(MapWidth-1, MapHeight-1))}");
    }

    // This is called when selection changes in the Buildings dropdown
    public void OnBuildingSelectionChange(int newSelection)
    {
        // 0 is always the header text
        if (newSelection >= 0 && newSelection < housePrefabs.Count)
        {
            if (activeCreation != null)
            {
                Unselect();
            }
            selectedBuildingIndex = newSelection;
            Create();
        }
    }

    public void Create()
    {
		activeCreation = Instantiate(housePrefabs[selectedBuildingIndex], Vector3.zero, Quaternion.identity, cityGrid.gameObject.transform);
		activeCreation.transform.position = Vector3.zero;
    }

    bool CanPlace(Vector2Int topLeft)
    {
        // Can the building be placed here?
        int w = BuildingWidth;
        int h = BuildingHeight;

        int x = topLeft.x;
        int y = topLeft.y;

        // Inside map
        hitBuilding = false;
        hitBorder = false;
        hitRoad = false;

        if (x >= 0 && y >= 0 &&
            x + w <= tileContents.GetLength(0) &&
            y + h <= tileContents.GetLength(1))
        {
            // Not over other building
            for (int ix = x; ix < x + w; ix++)
            {
                if (hitBuilding) { break; }
                for (int iy = y; iy < y + h; iy++)
                {
                    if (tileContents[ix, iy] == 1)
                    {
                        hitBuilding = true;
                        break;
                    }
                }
            }
        }
        else
        {
            hitBorder = true;
        }

        // Check for road tiles
        Vector3Int cellXY = GetCellUnderMouse();
        roadCheck = $"RC: {cellXY.x}->{cellXY.x+w} {cellXY.y}->{cellXY.y-h}";

        for (int tx = cellXY.x; tx < cellXY.x + w; tx++)
        {
            // Y decreases for some reason
            for (int ty = cellXY.y; ty > cellXY.y - h; ty--)
            {
                Vector3Int p = new Vector3Int(tx, ty);
                if (canBuildSprites.Contains(baseMap.GetSprite(p)) == false)
				{
                    firstRoadHit = new Vector2Int(p.x, p.y);
                    hitRoad = true;
                    break;
                }
			}
            if (hitRoad) { break; }
        }

        return (hitBuilding == false && hitBorder == false && hitRoad == false);
    }
    
    Sprite? GetSpriteUnderMouse()
    {
        Vector3Int wt = GetCellUnderMouse();
        return baseMap.GetSprite(wt);
    }

    void PlaceActive()
    {
        Vector2Int at = GetArrayTileUnderMouse();

        if (CanPlace(at))
        {
            // All are free
            // Places the building
            activeCreation = null;
            for (int ix = at.x; ix < at.x + BuildingWidth; ix++)
            {
                for (int iy = at.y; iy < at.y + BuildingHeight; iy++)
                {
                    tileContents[ix, iy] = 1;
                }
            }
			HousePlacedEvent(selectedBuildingIndex);
            // reselects the building type
            Create();
            return;
        }
    }

    void Unselect()
    {
		Destroy(activeCreation);
        activeCreation = null;
    }

    Vector2Int GetWorldTileUnderMouse()
    {
        Vector3 mp = Input.mousePosition;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mp);
        // Mouse starts from 0.0f
        // When World Tile y is 0, mouse.y is between 0 and -1
        int mx = (int)Mathf.Floor(mouseWorld.x);
        float my = mouseWorld.y;
        return new Vector2Int(mx, (int)my);
    }

    Vector3Int GetCellUnderMouse()
    {
        Vector3 mp = Input.mousePosition;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mp);
        return baseMap.WorldToCell(mouseWorld);
    }

    Vector2Int GetArrayTileUnderMouse()
    {
        return WorldTileToArray(GetWorldTileUnderMouse());
    }

    Vector2Int ArrayTileToWorld(Vector2Int tile)
    {
        // In world Y increases up
        return new Vector2Int(tile.x, -tile.y);
    }

    Vector2Int WorldTileToArray(Vector2Int tile)
    {
        // mouse tiles are [-size/2, size/2-1]
        // Array tiles are [0,size[
        // In array y increases down
        int my = 0;
		my = tile.y * -1;
        return new Vector2Int(tile.x, my);
    }

    void DrawCellDebug(Vector2Int worldTile)
    {
        Vector3 snap = new Vector3(worldTile.x, worldTile.y - GridSizeUnits);
        Vector3 tr = snap + new Vector3(GridSizeUnits, GridSizeUnits, 0.0f);
        Vector3 tl = snap + new Vector3(0.0f, GridSizeUnits, 0.0f);
        Vector3 br = snap + new Vector3(GridSizeUnits, 0.0f, 0.0f);
        Debug.DrawLine(snap, tr);
        Debug.DrawLine(tl, br);
    }

    // Update is called once per frame
    void Update()
    {
        wtum = GetWorldTileUnderMouse();
        DrawCellDebug(wtum);
		for (int x = 0; x < MapWidth; x++)
        {
            for (int y= 0; y < MapHeight; y++)
            {
                if (tileContents[x, y] == 1)
                {
                    Vector2Int wt = ArrayTileToWorld(new Vector2Int(x, y));
                    DrawCellDebug(wt);
                }
            }
        }

        if (activeCreation != null)
        {
            // Snap buildings to grid
            // starting from origo
            activeCreation.transform.position = new Vector3(wtum.x, wtum.y);
            CanPlace(GetArrayTileUnderMouse());

            if (Input.GetMouseButtonDown((int)MouseButton.Right))
            {
                Unselect();
            }
            if (Input.GetMouseButtonDown((int)MouseButton.Left))
            {
                PlaceActive();
            }
        }
    }
	private void OnGUI()
	{
        MenuCreator placeMenu = new MenuCreator(20, 80, 300, 20);
        placeMenu.Label($"Cell {gridX},{gridY}");
        placeMenu.Label($"Mouse W {wtum.x},{wtum.y}");
        placeMenu.Label($"Mouse W2 {wtum.x + BuildingWidth}, {wtum.y + BuildingHeight}");
        Vector2Int mouseArray = WorldTileToArray(GetWorldTileUnderMouse());
        placeMenu.Label($"Mouse A {mouseArray.x}/{MapWidth}, {mouseArray.y}/{MapHeight}");
        placeMenu.Label($"Mouse A2 {mouseArray.x + BuildingWidth}/{MapWidth}, {mouseArray.y + BuildingHeight}/{MapHeight}");
        placeMenu.Label($"Hit Building {hitBuilding} Hit Border {hitBorder} Hit Road {hitRoad}");

        Vector3Int cellmouse = GetCellUnderMouse();
        placeMenu.Label($"Cell: {cellmouse.x}, {cellmouse.y}");
        placeMenu.Label(roadCheck);
        Sprite? spum = GetSpriteUnderMouse();
        if (spum != null)
        {
            placeMenu.Label($"Sprite: {GetSpriteUnderMouse().ToString()}");
            placeMenu.CheckBox("CanBuild", canBuildSprites.Contains(spum));
        }
        placeMenu.Label($"First hit {firstRoadHit}");

    }
}
