﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Windows.Speech;

// Import the AStar_2D namespace
using AStar_2D;
using AStar_2D.Pathfinding;
using System;

// Namespace
namespace AStar_2D.Demo
{
    /// <summary>
    /// Inherits the AStar class that allows the user to specify what INode to use for pathfinding, in this case Tile.
    /// By default, AIManager is a singleton which can be accessed anywhere in code using AIManager.Instance.
    /// This allows access to the pathfinding methods within.
    /// </summary>
	public class TileManager : AStarGrid
    {
        // Private
        private Tile[,] tiles;
        private Path currentPath = null;
        private WeightPainter painter = null;
        public KeywordRecognizer keywordRecognizer;
        public KeywordRecognizer keywordRecognizer2;
        private Dictionary<string, Action> actions = new Dictionary<string, Action>();
        private List<String> dic = new List<String>();
        private int x = 0;
        private int y = 0;
        private int tileX = 1;
        private int tileY = 1;
        Agent agentS = new Agent();
        private bool mostrarTelaraña = false;
            
        

        // Public
        /// <summary>
        /// How many tiles to create in the X axis.
        /// </summary>
        private int gridX = 0;
        /// <summary>
        /// How many tiles to create in the Y axis.
        /// </summary>
        private int gridY = 0;
        /// <summary>
        /// The prefab that represents an individual tile.
        /// </summary>
        public GameObject tilePrefab;
        /// <summary>
        /// When true, a preview path will be shown when the mouse hovers over a tile.
        /// </summary>
        public bool showPreviewPath = false;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// Note that the base method is called. This is essential to ensure that the base class initializes correctly.
        /// </summary>
        //public override void Awake() {  }
        public Sprite blocked;
        public void Inicio()
        {
            base.Awake();

            tiles = new Tile[gridX, gridY];

            for (int i = 0; i < gridX; i++)
            {
                for (int j = 0; j < gridY; j++)
                {
                    // Create the tile at its location
                    GameObject obj = MonoBehaviour.Instantiate(tilePrefab, new Vector3((i - (gridX / 2)) * 0.6f, (j - (gridY / 2)) * 0.6f), Quaternion.identity) as GameObject;

                    // Add the tile script
                    tiles[i, j] = obj.GetComponent<Tile>();
                    tiles[i, j].index = new Index(i, j);

                    // Add an event listener
                    tiles[i, j].onTileSelected += onTileSelected;

                    // Check for preview
                    if (showPreviewPath == true)
                        tiles[i, j].onTileHover += onTileHover;

                    // Add the tile as a child to keep the scene view clean
                    obj.transform.SetParent(transform);
                }
            }

            

                    //tiles[3, 3].diagonalMode = PathNodeDiagonalMode.NoDiagonal;

            // Pass the arry to the search grid
            constructGrid(tiles);

            blocked = Resources.Load<Sprite>("blocked");              int percentage = (int)((gridY * gridX) * (0.25));             for (int i = 0; i < percentage; i++)             {                 int x = (int)UnityEngine.Random.Range(0, gridX);                 int y = (int)UnityEngine.Random.Range(0, gridY);                 tiles[x, y].IsWalkable = false;                 tiles[x, y].GetComponent<SpriteRenderer>().sprite = blocked;              }
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Try to find the painter
            agentS = Component.FindObjectOfType<Agent>();
            painter = Component.FindObjectOfType<WeightPainter>();
            actions.Add("Mover", Move);
            actions.Add("Crear ciudad", CreateCity);
            actions.Add("Elegir Destino", destinoCity);
            actions.Add("Mostrar telaraña", showWeb);
            actions.Add("Ocultar telaraña", hideWeb);
            for (int i = 1; i < 30; i++)
            {
                dic.Add(Convert.ToString(i));
            }

            keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;


            keywordRecognizer2 = new KeywordRecognizer(dic.ToArray());
            
            keywordRecognizer.Start();
        }
        private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
        {
            Debug.Log(speech.text);
            actions[speech.text].Invoke();
        }

        private void RecognizedSpeechX(PhraseRecognizedEventArgs speech)
        {
            //FindObjectOfType<AudioManager>().Play("SpidermanDestinoColumna");
            Debug.Log(speech.text);
            x = Convert.ToInt32(speech.text);
            keywordRecognizer2.OnPhraseRecognized -= RecognizedSpeechX;
            keywordRecognizer2.OnPhraseRecognized += RecognizedSpeechY;
        }
        private void RecognizedSpeechY(PhraseRecognizedEventArgs speech)
        {
            Debug.Log(speech.text);
            y = Convert.ToInt32(speech.text);
            keywordRecognizer2.Stop();
            Mueve(x, y);
            keywordRecognizer2.OnPhraseRecognized -= RecognizedSpeechY;
        }
        private void RecognizedCitySizeX(PhraseRecognizedEventArgs speech)
        {
            Debug.Log(speech.text);
            gridX = Convert.ToInt32(speech.text);
            keywordRecognizer2.OnPhraseRecognized -= RecognizedCitySizeX;
            keywordRecognizer2.OnPhraseRecognized += RecognizedCitySizeY;
        }
        private void RecognizedCitySizeY(PhraseRecognizedEventArgs speech)
        {
            Debug.Log(speech.text);
            gridY = Convert.ToInt32(speech.text);
            keywordRecognizer2.Stop();
            Inicio();
            keywordRecognizer2.OnPhraseRecognized -= RecognizedCitySizeY;
        }

        private void RecognizedShowWeb(PhraseRecognizedEventArgs speech)
        {
            keywordRecognizer2.Stop();
            Debug.Log(speech.text);
            mostrarTelaraña = true;
            onTileHover(ReturnTile(tileX, tileY));
            Update();
            keywordRecognizer2.OnPhraseRecognized -= RecognizedShowWeb;
        }

        private void RecognizedHideWeb(PhraseRecognizedEventArgs speech)
        {
            keywordRecognizer2.Stop();
            Debug.Log(speech.text);
            mostrarTelaraña = false;
            onTileHover(ReturnTile(tileX, tileY));
            Update();
            keywordRecognizer2.OnPhraseRecognized -= RecognizedHideWeb;
        }

        private void Move()
        {
            //FindObjectOfType<AudioManager>().Play("SpidermanDestinoFila");
            keywordRecognizer2.OnPhraseRecognized += RecognizedSpeechX;
            keywordRecognizer2.Start();
        }
        private void CreateCity()
        {
            //FindObjectOfType<AudioManager>().Play("SpidermanDestinoFila");
            keywordRecognizer2.OnPhraseRecognized += RecognizedCitySizeX;
            keywordRecognizer2.Start();
        }

        private void destinoCity()
        {
            //FindObjectOfType<AudioManager>().Play("SpidermanDestinoFila");
            keywordRecognizer2.OnPhraseRecognized += RecognizedDestinationSizeX;
            keywordRecognizer2.Start();
        }
        private void showWeb()
        {
            //FindObjectOfType<AudioManager>().Play("SpidermanDestinoFila");
            keywordRecognizer2.OnPhraseRecognized += RecognizedShowWeb;
            keywordRecognizer2.Start();
        }
        private void hideWeb()
        {
            //FindObjectOfType<AudioManager>().Play("SpidermanDestinoFila");
            keywordRecognizer2.OnPhraseRecognized += RecognizedHideWeb;
            keywordRecognizer2.Start();
        }

        public Sprite building;
        private void RecognizedDestinationSizeX(PhraseRecognizedEventArgs speech)
        {
            Debug.Log(speech.text);
            building = Resources.Load<Sprite>("building");
            ReturnTile(tileX - 1, tileY - 1).GetComponent<SpriteRenderer>().sprite = building;
            tileX = Convert.ToInt32(speech.text);
            keywordRecognizer2.OnPhraseRecognized -= RecognizedDestinationSizeX;
            keywordRecognizer2.OnPhraseRecognized += RecognizedDestinationSizeY;
        }
        public Sprite final;
        private void RecognizedDestinationSizeY(PhraseRecognizedEventArgs speech)
        {
            Debug.Log(speech.text);
            tileY = Convert.ToInt32(speech.text);
            keywordRecognizer2.Stop();
            painter = null;
            onTileHover(ReturnTile(tileX-1, tileY-1));

            final = Resources.Load<Sprite>("final");

            ReturnTile(tileX - 1, tileY - 1).GetComponent<SpriteRenderer>().sprite = final;


            keywordRecognizer2.OnPhraseRecognized -= RecognizedDestinationSizeY;
        }

        private void Mueve(int x, int y)
        {
            if (x > 0)
            {
                x -= 1;
            }
            if (y > 0)
            {
                y -= 1;
            }
            agentS.transform.position = new Vector3(ReturnTile(x,y).transform.position.x,(ReturnTile(x, gridY - y).transform.position.y), 0);
            
        }

        /// <summary>
        /// Called by Unity.
        /// Left blank for demonstration.
        /// </summary>
        public override void Update()
        {
            // Call base method
            base.Update();

            // Do stuff
            if(currentPath != null)
            {
                if(currentPath.IsFullyReachable == false)
                {
                    // Find the first agent
                    Agent agent = Component.FindObjectOfType<Agent>();
                

                    if (agent != null)
                    {
                        Index current = findNearestIndex(agent.transform.position);

                        // Request a path but dont assign it to the agent - this will allow the preview to be shown without the agent following it
                        findPath(current, currentPath.LastNode.Index, (Path result, PathRequestStatus status) =>
                        {
                            currentPath = result;
                        });
                    }
                }
            }
        }
        
        private void onTileSelected(Tile tile, int mouseButton)
        {
            // Check for a valid painter
            if (painter != null)
            {
                // Check if we are using the painter - if so, dont bother finding paths
                if (painter.IsPainting == true)
                    return;
            }

            // Check for button
            if (mouseButton == 0)
            {
                // Set the destination
                Agent[] agents = Component.FindObjectsOfType<Agent>();

                // Set the target for all agents
                foreach (Agent agent in agents)
                    agent.setDestination(tile.WorldPosition);
            }
            else if(mouseButton == 1)
            {
                // Toggle the walkable status
                tile.toggleWalkable();
            }
        }

        private Tile ReturnTile(int x, int y) {

            Tile temp;
            temp = tiles[x,y];
            return temp;

        }

        private void onTileHover(Tile tile)
        {
            // Check for a valid painter
            if(painter != null)
            {
                // Check if we are using the painter - if so, dont bother finding paths
                if(painter.IsPainting == true)
                    return;
            }


            // Find the first agent
            Agent agent = Component.FindObjectOfType<Agent>();

            if (true)
            {

                if (agent != null)
                {
                    // Find the tile index
                    Index current = findNearestIndex(agent.transform.position);
                    // Request a path but dont assign it to the agent - this will allow the preview to be shown without the agent following it                
                    findPath(current, tile.index, (Path result, PathRequestStatus status) =>
                    {
                        currentPath = result;
                    });
                }
            }
        }
    }
}