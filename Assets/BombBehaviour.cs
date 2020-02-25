using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombBehaviour : PhysicsObject
{
    [Header("Physics")]
    [Range(0,1), SerializeField] private float GroundFriction = 0;
    [SerializeField] private float airDrag = 1;
    [SerializeField] private float bouncePower = 0;
    [HideInInspector] public Vector2 tossDirection;

    [Header("Bomb Explosion")]
    [SerializeField] private float explosionRadius = 30;
    [SerializeField, Tooltip("in seconds")] private float explosionTime = 3;
    [SerializeField] private ParticleSystem bombParticles = null;
    [SerializeField] private ParticleSystem tileParticles = null;
    [SerializeField] private float screenShakeDuration = 0.1f;

    private Vector2 _tossVelocity; // X velocity decreased over time;
    private float _timeCount;
    private bool _exploded = false;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        this.velocity.y = tossDirection.y;
        _tossVelocity = tossDirection;
        _timeCount = explosionTime;
        _spriteRenderer = this.GetComponent<SpriteRenderer>();
        _spriteRenderer.material = new Material(_spriteRenderer.material);
    }

    protected override void ComputeVelocity()
    {
        if(Input.GetKeyDown(KeyCode.Delete)) Destroy(this.gameObject);
        this.targetVelocity.x = _tossVelocity.x;
        _tossVelocity.x *= 1 - Time.deltaTime * airDrag;
        _tossVelocity.y = this.velocity.y;



        _timeCount -= Time.deltaTime;
        if(_timeCount < 1 && !_exploded)
            _spriteRenderer.material.SetFloat("_FlashAmount", 1 - _timeCount);
        // after the time count, explode
        if(_timeCount < 0 && !_exploded)
        {
            _exploded = true;
            if(bombParticles) Instantiate(bombParticles, transform.position, Quaternion.identity);
            Collider2D[] cols = Physics2D.OverlapCircleAll(this.transform.position, explosionRadius);
            foreach(Collider2D col in cols)
            {
                //must check for tilemaps to remove specified tile
                Tilemap tilemap = col.GetComponent<Tilemap>();  
                if(tilemap)
                {
                    StartCoroutine(tilemapExplosion(tilemap, transform.position));
                }
            }
            Destroy(this.GetComponent<Collider2D>());
            Destroy(_spriteRenderer);
            Destroy(this.gameObject, 1f);
            rb2d.simulated = false;
            Camera.main.GetComponent<CameraBehaviour>().Shake(screenShakeDuration);
            Camera.main.GetComponent<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
        }
    }

    protected override void OnTouchGround()
    {
        // Vertical Bounce
        this.velocity.y = -_tossVelocity.y * bouncePower;
        _tossVelocity.x *= 1 - GroundFriction;
    }

    protected override void OnCollision(RaycastHit2D hit)
    {
        // Horizontal Bounce
        if(1 - Mathf.Abs(hit.normal.x) <= 0.01){
            _tossVelocity.x = -_tossVelocity.x * bouncePower;
        } 
    }

    IEnumerator gridExplosion(int gridSize, Vector2Int root)
    {
        /* this function fills a grid from its center to its corner iteratively */

        //input grid has size nxn
        int n = gridSize;
        bool[,] grid = new bool[n, n];
        //_n is n iterative size
        int ri = root.y;
        int rj = root.x;
        for(int _n = 0; _n < n; n++)
        {
            for(int i = -_n; i < _n; i++)
            {
                grid[ri + i, rj - _n] = true;
                grid[ri + i, rj + _n] = true;
                grid[ri - _n, rj + i] = true;
                grid[ri + _n, rj + i] = true;
            }
        }
        yield return null;
    }

    IEnumerator tilemapExplosion(Tilemap tilemap, Vector3 root)
    {
        WaitForSeconds waitTime = new WaitForSeconds(0.15f);
        Vector3 gridCellSize = tilemap.layoutGrid.cellSize;

        //input grid has size nxn
        int n = Mathf.FloorToInt(explosionRadius * 2) + 2; // Adding 2 for borders
        //_n is n iterative size
        float ri = root.x;
        float rj = root.y;
        for(int _n = 0; _n < n; _n++)
        {
            for(int i = -_n; i <= _n; i++)
            {
                DestroyTile(tilemap, new Vector3(ri + i * gridCellSize.x, rj - _n * gridCellSize.y) );
                DestroyTile(tilemap, new Vector3(ri + i * gridCellSize.x, rj + _n * gridCellSize.y) );
                DestroyTile(tilemap, new Vector3(ri - _n * gridCellSize.x, rj + i * gridCellSize.y) );
                DestroyTile(tilemap, new Vector3(ri + _n * gridCellSize.x, rj + i * gridCellSize.y) );
            }
            yield return waitTime;
        }
        Destroy(this.gameObject);
    }

    private void DestroyTile(Tilemap tilemap, Vector3 pos){
        //GameObject cube = GameObject.Find("Cube");
        //Instantiate(cube, pos, Quaternion.identity);
        Vector3Int tilePos = tilemap.WorldToCell(pos);
        Vector3 centerPos = tilemap.GetCellCenterWorld(tilePos);
        if(Vector2.Distance(centerPos, transform.position) > explosionRadius) return; // return if out of radius
        if(tilemap.GetTile(tilePos))
        {   
            Instantiate(tileParticles, pos, Quaternion.identity);
            tilemap.SetTile(tilePos, null);
            tilemap.RefreshTile(tilePos);
        }
    }

    // Gizmos stuff
    private void OnDrawGizmos(){
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

}
