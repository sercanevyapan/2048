using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private List<BlockType> _types;
    [SerializeField] private float _travelTime = 0.2f;
    [SerializeField] private int _winCondition = 2048;

    [SerializeField] private GameObject _winScreen, _loseScreen ;
    [SerializeField] private TMP_Text _highScore;

    [SerializeField] private Score score;


    private List<Node> _nodes;
    private List<Block> _blocks;
    private GameState _state;
    private int _round;

    private BlockType GetBlockTypeByValue(int value) => _types.First(t => t.Value == value);




    private void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState)
    {
        _state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                GetScoreInfo();
                break;
            case GameState.SpawingBlocks:
                SpawnBlocks(_round++==0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
               
                break;
            case GameState.Moving:
           
                break;
            case GameState.Win:
                _winScreen.SetActive(true);
           
                SetScoreInfo(score);

                score.gameObject.SetActive(false);
                break;
            case GameState.Lose:
                _loseScreen.SetActive(true);
        
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void Update()
    {
        if (_state != GameState.WaitingInput) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
    }

    void GenerateGrid()
    {
        _round = 0;
        _nodes = new List<Node>();
        _blocks = new List<Block>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity);
                _nodes.Add(node);
            }
        }
        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);

        var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(_width, _height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawingBlocks);
    }

    void SpawnBlocks(int amount)
    {
        var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        foreach (var node in freeNodes.Take(amount))
        {
            SpawnBlock(node, Random.value > 0.8f ? 4 : 2);
        }
       


        if (freeNodes.Count() == 1)
        {
            ChangeState(GameState.Lose);
            return;
        }

        ChangeState(_blocks.Any(b=> b.Value == _winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    void SpawnBlock(Node node, int value)
    {
        var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(node);
        _blocks.Add(block);
    }

    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);

        var orderBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderBlocks.Reverse();

        foreach (var block in orderBlocks)
        {
            var next = block.Node;
            do
            {
                block.SetBlock(next);

                var possibleNode = GetNodeAtPosition(next.Pos + dir);
                if(possibleNode != null)
                {
                    if(possibleNode.OccupiedBlock !=null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
                        block.MergeBlock(possibleNode.OccupiedBlock);

                    }

                    else if (possibleNode.OccupiedBlock == null) next = possibleNode;
                }



            } while (next != block.Node);


            
        }


        var sequence = DOTween.Sequence();

        foreach (var block in orderBlocks)
        {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;

            sequence.Insert(0, block.transform.DOMove(movePoint, _travelTime));

        }

        sequence.OnComplete(() =>
        {
            foreach (var block in orderBlocks.Where(b=> b.MergingBlock !=null))
            {
                MergeBlocks(block.MergingBlock, block );
            }

            ChangeState(GameState.SpawingBlocks);
        });

    }

    void MergeBlocks(Block baseBlock, Block merginBlock)
    {
        SpawnBlock(baseBlock.Node, baseBlock.Value *2);

        RemoveBlock(baseBlock);
        RemoveBlock(merginBlock);
    }
   
    void RemoveBlock(Block block)
    {
        _blocks.Remove(block);
        Destroy(block.gameObject);
    }

    Node GetNodeAtPosition(Vector2 pos)
    {
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }

  
    private void SetScoreInfo(Score score)
    {
        Debug.Log(score.scoreAmount);
        int oldScore = PlayerPrefs.GetInt("Score");
        if (score.scoreAmount<oldScore||oldScore==0)
            PlayerPrefs.SetInt("Score", (int)score.scoreAmount);

        GetScoreInfo();
    }

    private void GetScoreInfo()
    {
        
            _highScore.text =  PlayerPrefs.GetInt("Score").ToString() + " High Score";
    }
}


[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
}

public enum GameState
{
    GenerateLevel,
    SpawingBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}