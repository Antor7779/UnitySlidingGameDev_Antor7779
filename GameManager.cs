using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // SerializeField allows you to modify these variables from the Unity Editor
    [SerializeField] private Transform gameTransform;  // The transform of the game object container
    [SerializeField] private Transform piecePrefab;    // The prefab for individual game pieces

    private List<Transform> pieces;  // List to store all game pieces

    // Variables to track the empty piece location and the size of the game
    private int emptyLocation;  // Index of the empty piece in the grid
    private int size;           // Size of the game grid (e.g., 3 for a 3x3 grid)

    private bool shuffling = false;  // Flag to indicate if the game is currently shuffling

    // This method creates the game pieces with a specified gap thickness
    private void CreateGamePieces(float gapThickness)
    {
        // Calculate the width of each piece based on the size of the game
        float width = 1 / (float)size;

        // Calculate the starting position to center the grid
        float startX = -1 + width;
        float startY = 1 - width;

        // Loop through each row and column to create the pieces
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                // Instantiate a new piece and set its parent to gameTransform
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);  // Add the piece to the pieces list

                // Set the local position of the piece based on its row and column
                piece.localPosition = new Vector3(startX + (2 * width * col),
                                                  startY - (2 * width * row),
                                                  0);

                // Set the scale of the piece, adjusting for the gap thickness
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;

                // Name the piece based on its position in the grid
                piece.name = $"{(row * size) + col}";

                // Hide the last piece to represent the empty location
                if ((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    // Calculate UV coordinates for texture mapping
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;

                    Vector2[] uv = new Vector2[4];

                    // UV coordinate order: (0,1), (0,0), (1,0), (1,1)
                    uv[0] = new Vector2((width * col) + gap, 1 - (width * (row + 1)) - gap);
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - (width * (row + 1)) - gap);
                    uv[2] = new Vector2((width * col) + gap, 1 - (width * row) + gap);
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - (width * row) + gap);
                    mesh.uv = uv;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pieces = new List<Transform>();  // Initialize the list of pieces
        // Set the size of the game and create the game pieces
        size = 3;
        CreateGamePieces(0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the game is not shuffling and if the game is complete
        if (!shuffling && CheckCompletion())
        {
            shuffling = true;  // Set shuffling flag to true
            StartCoroutine(WaitShuffle(0.5f));  // Start shuffling after a delay
        }

        // Check for mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast to detect the clicked piece
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                // Find the clicked piece in the pieces list
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == hit.transform)
                    {
                        // Check if the piece can be swapped with the empty piece
                        if (SwapIfValid(i, -size, size)) { break; }
                        if (SwapIfValid(i, +size, size)) { break; }
                        if (SwapIfValid(i, -1, 0)) { break; }
                        if (SwapIfValid(i, +1, size - 1)) { break; }
                    }
                }
            }
        }
    }

    // Method to swap the clicked piece with the empty piece if the move is valid
    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        // Check if the move is valid based on the grid constraints
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            // Swap the pieces in the list
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);

            // Swap their positions
            (pieces[i].localPosition, pieces[i + offset].localPosition) = (pieces[i + offset].localPosition, pieces[i].localPosition);

            // Update the empty piece location
            emptyLocation = i;
            return true;
        }
        return false;
    }

    // Method to check if the puzzle is complete
    private bool CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;  // Return false if any piece is out of order
            }
        }
        return true;  // Return true if all pieces are in order
    }

    // Coroutine to wait before shuffling
    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);  // Wait for the specified duration
        Shuffle();  // Shuffle the pieces
        shuffling = false;  // Reset shuffling flag
    }

    // Method to shuffle the pieces
    private void Shuffle()
    {
        int count = 0;  // Counter to keep track of the number of swaps
        int last = 0;   // Variable to store the last shuffled piece

        while (count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);  // Generate a random index

            if (rnd == last) { continue; }  // Skip if the same piece is selected

            last = emptyLocation;  // Update the last shuffled piece

            // Try to swap the pieces and increment the counter if successful
            if (SwapIfValid(rnd, -size, size))
            {
                count++;
            }
            else if (SwapIfValid(rnd, +size, size))
            {
                count++;
            }
            else if (SwapIfValid(rnd, -1, 0))
            {
                count++;
            }
            else if (SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }
}
