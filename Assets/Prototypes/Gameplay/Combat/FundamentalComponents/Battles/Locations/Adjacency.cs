using System.Collections.Generic;
using Assets.Prototypes.Characters;
using Assets.Prototypes.Skills.Nodes;
using UnityEngine;

public enum Direction
{
    Center,
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
}

public class Adjacency
{
    // Center position (the unit itself)
    public CharacterInstance Center { get; set; }

    // Eight adjacent positions
    public CharacterInstance TopLeft { get; set; }
    public CharacterInstance TopCenter { get; set; }
    public CharacterInstance TopRight { get; set; }
    public CharacterInstance CenterLeft { get; set; }
    public CharacterInstance CenterRight { get; set; }
    public CharacterInstance BottomLeft { get; set; }
    public CharacterInstance BottomCenter { get; set; }
    public CharacterInstance BottomRight { get; set; }

    public Adjacency() { }

    // Get unit at specific direction
    public CharacterInstance GetUnit(Direction direction)
    {
        return direction switch
        {
            Direction.Center => Center,
            Direction.TopLeft => TopLeft,
            Direction.TopCenter => TopCenter,
            Direction.TopRight => TopRight,
            Direction.CenterLeft => CenterLeft,
            Direction.CenterRight => CenterRight,
            Direction.BottomLeft => BottomLeft,
            Direction.BottomCenter => BottomCenter,
            Direction.BottomRight => BottomRight,
            _ => null,
        };
    }

    // Set unit at specific direction
    public void SetUnit(Direction direction, CharacterInstance unit)
    {
        switch (direction)
        {
            case Direction.Center:
                Center = unit;
                break;
            case Direction.TopLeft:
                TopLeft = unit;
                break;
            case Direction.TopCenter:
                TopCenter = unit;
                break;
            case Direction.TopRight:
                TopRight = unit;
                break;
            case Direction.CenterLeft:
                CenterLeft = unit;
                break;
            case Direction.CenterRight:
                CenterRight = unit;
                break;
            case Direction.BottomLeft:
                BottomLeft = unit;
                break;
            case Direction.BottomCenter:
                BottomCenter = unit;
                break;
            case Direction.BottomRight:
                BottomRight = unit;
                break;
        }
    }

    // Get all non-null adjacent units (excluding center)
    public IEnumerable<CharacterInstance> GetAllAdjacent()
    {
        if (TopLeft != null)
            yield return TopLeft;
        if (TopCenter != null)
            yield return TopCenter;
        if (TopRight != null)
            yield return TopRight;
        if (CenterLeft != null)
            yield return CenterLeft;
        if (CenterRight != null)
            yield return CenterRight;
        if (BottomLeft != null)
            yield return BottomLeft;
        if (BottomCenter != null)
            yield return BottomCenter;
        if (BottomRight != null)
            yield return BottomRight;
    }

    // get adjacent allies
    public IEnumerable<CharacterInstance> GetAdjacentAllies(SkillExecutionContext context)
    {
        foreach (var adjacent in GetAllAdjacent())
        {
            if (
                adjacent != null
                && context.Allies != null
                && context.Allies.Exists(ally => ally.Id == adjacent.Id)
            )
            {
                yield return adjacent;
            }
        }
    }

    // get adjacent enemies
    public IEnumerable<CharacterInstance> GetAdjacentEnemies(SkillExecutionContext context)
    {
        foreach (var adjacent in GetAllAdjacent())
        {
            if (
                adjacent != null
                && context.Targets != null
                && context.Targets.Exists(target => target.Id == adjacent.Id)
            )
            {
                yield return adjacent;
            }
        }
    }

    // get adjacent ally count
    public int GetAdjacentAllyCount(SkillExecutionContext context)
    {
        int count = 0;
        foreach (var adjacent in GetAllAdjacent())
        {
            if (
                adjacent != null
                && context.Allies != null
                && context.Allies.Exists(ally => ally.Id == adjacent.Id)
            )
            {
                count++;
            }
        }
        return count;
    }

    // get adjacent enemy count
    public int GetAdjacentEnemyCount(SkillExecutionContext context)
    {
        int count = 0;
        foreach (var adjacent in GetAllAdjacent())
        {
            if (
                adjacent != null
                && context.Targets != null
                && context.Targets.Exists(target => target.Id == adjacent.Id)
            )
            {
                count++;
            }
        }
        return count;
    }
}
