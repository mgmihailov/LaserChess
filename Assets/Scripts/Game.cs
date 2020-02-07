using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public UserInterface UserInterface;

    public GameObject[] PiecesPrefabs = new GameObject[6];

    private const int LEVELS = 3;

    private Player[] players;

    private List<List<SquareInitData>> boardConfigs;

    private int currentPlayerIdx;

    private int currentLevel;

    private Phase gamePhase;


    enum PieceType
    {
        Grunt = 0,
        JumpShip,
        Tank,
        Drone,
        Dreadnought,
        CommandUnit
    }

    enum Phase
    {
        Started = 0,
        Playing,
        Reset,
        Over
    }


    public void OnUINextLevelButtonClick()
    {
        ++currentLevel;
        ResetGame();
        gamePhase = Phase.Reset;
    }

    public void OnUIRestartLevelButtonClick()
    {
        currentLevel = 0;
        ResetGame();
        gamePhase = Phase.Reset;
    }

    public void OnUIQuitButtonClick()
    {
        EndGame();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentLevel = 0;
        boardConfigs = new List<List<SquareInitData>>();
        for (int i = 0; i < LEVELS; ++i)
        {
            GenerateBoardConfig(i);
        }

        Piece.OnAttack += OnPieceAttack;

        players = new Player[2];
        players[0] = GetComponent<HumanPlayer>();
        players[0].OnVictory += OnPlayerVictory;
        players[1] = GetComponent<AIPlayer>();
        players[1].OnVictory += OnPlayerVictory;

        currentPlayerIdx = Random.Range(0, 2);

        UserInterface.Initialize(players[0] as HumanPlayer);
        UserInterface.AddOnNextLevelButtonClickListener(OnUINextLevelButtonClick);
        UserInterface.AddOnRestartButtonClickListener(OnUIRestartLevelButtonClick);
        UserInterface.AddOnQuitButtonClickListener(OnUIQuitButtonClick);

        GameObject gameBoard = GameObject.FindGameObjectWithTag("Board");
        gameBoard.GetComponent<Board>().Initialize();
        players[0].Initialize();
        players[1].Initialize();

        InitializeBoard();

        gamePhase = Phase.Started;
    }

    // Update is called once per frame
    void Update()
    {
        if (gamePhase == Phase.Over)
        {
            return;
        }

        if (gamePhase == Phase.Reset)
        {
            StartGame();
            gamePhase = Phase.Started;
            return;
        }

        if (gamePhase == Phase.Started)
        {
            players[currentPlayerIdx].Play();
            gamePhase = Phase.Playing;
            return;
        }

        if (!players[currentPlayerIdx].HasFinishedTurn())
        {
            return;
        }

        currentPlayerIdx = (currentPlayerIdx + 1) % 2;
        players[currentPlayerIdx].Play();
    }

    void GenerateBoardConfig(int level)
    {
        List<SquareInitData> config = new List<SquareInitData>();

        switch (level)
        {
            case 0:
                {
                    // Human pieces

                    // Grunts
                    for (int i = 0; i < Board.Columns; ++i)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Grunt,
                            ownership = Piece.Ownership.Human,
                            gridPosition = new Vector2Int(Board.Rows - 2, i),
                        });
                    }

                    // Tanks
                    config.Add(new SquareInitData
                    {
                        type = PieceType.Tank,
                        ownership = Piece.Ownership.Human,
                        gridPosition = new Vector2Int(Board.Rows - 1, 0),
                    });
                    config.Add(new SquareInitData
                    {
                        type = PieceType.Tank,
                        ownership = Piece.Ownership.Human,
                        gridPosition = new Vector2Int(Board.Rows - 1, 1),
                    });
                    config.Add(new SquareInitData
                    {
                        type = PieceType.Tank,
                        ownership = Piece.Ownership.Human,
                        gridPosition = new Vector2Int(Board.Rows - 1, Board.Columns - 1),
                    });
                    config.Add(new SquareInitData
                    {
                        type = PieceType.Tank,
                        ownership = Piece.Ownership.Human,
                        gridPosition = new Vector2Int(Board.Rows - 1, Board.Columns - 2),
                    });

                    // Jumpships
                    config.Add(new SquareInitData
                    {
                        type = PieceType.JumpShip,
                        ownership = Piece.Ownership.Human,
                        gridPosition = new Vector2Int(Board.Rows - 1, 2),
                    });
                    config.Add(new SquareInitData
                    {
                        type = PieceType.JumpShip,
                        ownership = Piece.Ownership.Human,
                        gridPosition = new Vector2Int(Board.Rows - 1, Board.Columns - 3),
                    });

                    // AI pieces

                    // Drones
                    for (int i = 0; i < Board.Columns; i += 2)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Drone,
                            ownership = Piece.Ownership.AI,
                            gridPosition = new Vector2Int(3, i),
                        });
                    }
                    for (int i = 1; i < Board.Columns; i += 2)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Drone,
                            ownership = Piece.Ownership.AI,
                            gridPosition = new Vector2Int(2, i),
                        });
                    }

                    // CommandUnits
                    config.Add(new SquareInitData
                    {
                        type = PieceType.CommandUnit,
                        ownership = Piece.Ownership.AI,
                        gridPosition = new Vector2Int(0, 3),
                    });
                }
                break;
            case 1:
                {
                    // Human pieces

                    // Grunts
                    for (int i = 0; i < Board.Columns; i += 2)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Grunt,
                            ownership = Piece.Ownership.Human,
                            gridPosition = new Vector2Int(Board.Rows - 2, i),
                        });
                    }

                    // JumpShips
                    for (int i = 1; i < Board.Columns; i += 2)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.JumpShip,
                            ownership = Piece.Ownership.Human,
                            gridPosition = new Vector2Int(Board.Rows - 2, i),
                        });
                    }

                    // AI pieces

                    // Drones
                    for (int i = 0; i < Board.Columns; ++i)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Drone,
                            ownership = Piece.Ownership.AI,
                            gridPosition = new Vector2Int(3, i),
                        });
                    }

                    // Dreadnoughts
                    config.Add(new SquareInitData
                    {
                        type = PieceType.Dreadnought,
                        ownership = Piece.Ownership.AI,
                        gridPosition = new Vector2Int(2, 3),
                    });
                    config.Add(new SquareInitData
                    {
                        type = PieceType.Dreadnought,
                        ownership = Piece.Ownership.AI,
                        gridPosition = new Vector2Int(2, 4),
                    });

                    // CommandUnits
                    config.Add(new SquareInitData
                    {
                        type = PieceType.CommandUnit,
                        ownership = Piece.Ownership.AI,
                        gridPosition = new Vector2Int(0, 3),
                    });
                }
                break;
            case 2:
                {
                    // Human pieces
                    for (int i = 0; i < Board.Columns; ++i)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Grunt,
                            ownership = Piece.Ownership.Human,
                            gridPosition = new Vector2Int(Board.Rows - 2, i),
                        });
                    }

                    // JumpShips
                    for (int i = 0; i < Board.Columns; i += 2)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.JumpShip,
                            ownership = Piece.Ownership.Human,
                            gridPosition = new Vector2Int(Board.Rows - 1, i),
                        });
                    }

                    // Tanks
                    for (int i = 1; i < Board.Columns; i += 2)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Tank,
                            ownership = Piece.Ownership.Human,
                            gridPosition = new Vector2Int(Board.Rows - 1, i),
                        });
                    }

                    // AI pieces

                    // Drones
                    for (int i = 0; i < Board.Columns; ++i)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Drone,
                            ownership = Piece.Ownership.AI,
                            gridPosition = new Vector2Int(3, i),
                        });
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Drone,
                            ownership = Piece.Ownership.AI,
                            gridPosition = new Vector2Int(2, i),
                        });
                    }

                    // Dreadnoughts
                    for (int i = 0; i < Board.Columns; i += 2)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.Dreadnought,
                            ownership = Piece.Ownership.AI,
                            gridPosition = new Vector2Int(1, i),
                        });
                    }

                    // CommandUnits
                    for (int i = 1; i < Board.Columns; i += 2)
                    {
                        config.Add(new SquareInitData
                        {
                            type = PieceType.CommandUnit,
                            ownership = Piece.Ownership.AI,
                            gridPosition = new Vector2Int(1, i),
                        });
                    }
                }
                break;
        }
        boardConfigs.Add(config);
    }

    void InitializeBoard()
    {
        foreach (var squareData in boardConfigs[currentLevel])
        {
            Transform square = Board.Instance.transform.GetChild(squareData.gridPosition.x * Board.Columns + squareData.gridPosition.y);
            Vector3 piecePos = square.position;
            piecePos.Set(piecePos.x, piecePos.y + PiecesPrefabs[(int)squareData.type].GetComponent<MeshRenderer>().bounds.size.y / 2, piecePos.z);
            GameObject obj = Instantiate(PiecesPrefabs[(int)squareData.type], piecePos, Quaternion.identity);

            Piece pieceComp = obj.GetComponent<Piece>();
            pieceComp.Initialize(squareData.gridPosition, squareData.ownership);
            pieceComp.pieceUi.Initialize();
            Board.Instance.GetComponent<Board>().AddPiece(obj, squareData.gridPosition);
            players[(int)squareData.ownership - 1].AddPiece(obj);
        }
    }

    void OnPieceAttack(Piece attacker, Vector2Int targetDestination)
    {
        Piece target = Board.Instance.GetSquare(targetDestination).Piece;
        target.ReceiveDamage(attacker.AttackPower);
    }

    void OnPlayerVictory(Player player)
    {
        gamePhase = Phase.Over;
        if (currentLevel < LEVELS - 1)
        {
            UserInterface.ShowEndLevelMenu(player.GetType().ToString());
        }
        else
        {
            UserInterface.ShowEndGameMenu();
        }
    }

    void StartGame()
    {
        GameObject gameBoard = GameObject.FindGameObjectWithTag("Board");
        gameBoard.GetComponent<Board>().Initialize();

        players[0].Initialize();
        players[1].Initialize();

        InitializeBoard();
    }

    void EndGame()
    {
        gamePhase = Phase.Over;
        Application.Quit();
    }

    void ResetGame()
    {
        gamePhase = Phase.Reset;
        players[0].Reset();
        players[1].Reset();

        Board.Instance.Reset();

        currentPlayerIdx = Random.Range(0, 2);
    }


    struct SquareInitData
    {
        public PieceType type;

        public Piece.Ownership ownership;

        public Vector2Int gridPosition;
    }
}
