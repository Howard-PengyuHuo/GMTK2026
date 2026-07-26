using UnityEngine;

// Attach to every child piece under TangledTubePieces. Set its shape and,
// for exactly one piece each, isStart / isEnd, in the Inspector.
//
// Shape openings at 0 turns (before any rotation), using N/E/S/W:
//   Straight  -> N & S open   (the two-sided straight tube)
//   Elbow     -> N & W open   (the L-shaped tube) e > w
//   TSection  -> W & E & S open (the 3-sided tube) n > w
// Rotating the piece clockwise shifts these openings clockwise too, so the
// actual open sides at runtime are computed from (base sides + currentTurns).
public class TangledPipePiece : MonoBehaviour
{
    public enum PipeType { Straight, Elbow, TSection }
    public enum ExternalDir { North, East, South, West } // matches your N/E/S/W index order

    [SerializeField] private PipeType type;
    [SerializeField] private bool isStart;
    [SerializeField] private bool isEnd;

    [Tooltip("Only used if isStart or isEnd is true ¡ª the side that must stay open " +
             "to connect to the pipe outside the grid.")]
    [SerializeField] private ExternalDir externalDirection;

    public PipeType Type => type;
    public bool IsStart => isStart;
    public bool IsEnd => isEnd;
    public int ExternalDirIndex => (int)externalDirection;
}