using Turnroot.Characters;
using Turnroot.Gameplay.Objects;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Properties for map grid points (tiles).
/// Includes preset properties for starting units and tile events.
/// </summary>
[System.Serializable]
[CreateAssetMenu(
    fileName = "New MapGridPointProperties",
    menuName = "Turnroot/Maps/Grid Point Properties"
)]
public class MapGridPointProperties : MapGridPropertyBase
{
    [Header("Grid Point Identity")]
    [Tooltip("Optional identifier for this grid point.")]
    public string pointId = string.Empty;

    [Tooltip("Optional friendly name for this grid point.")]
    public string pointName = string.Empty;

    // Constants for preset property keys
    public const string KEY_STARTING_UNIT = "startingUnit";
    public const string KEY_FRIENDLY_ENTERS = "friendlyEnters";
    public const string KEY_ENEMY_ENTERS = "enemyEnters";

    /// <summary>
    /// Initialize preset properties for a grid point.
    /// Should be called when creating a new grid point.
    /// </summary>
    public void InitializePresetProperties()
    {
        // Initialize Starting Unit property
        if (unitProperties.Find(p => p.key == KEY_STARTING_UNIT) == null)
        {
            unitProperties.Add(new UnitProperty { key = KEY_STARTING_UNIT, value = null });
        }

        // Initialize Friendly Enters event
        if (eventProperties.Find(p => p.key == KEY_FRIENDLY_ENTERS) == null)
        {
            eventProperties.Add(
                new EventProperty { key = KEY_FRIENDLY_ENTERS, value = new UnityEvent() }
            );
        }

        // Initialize Enemy Enters event
        if (eventProperties.Find(p => p.key == KEY_ENEMY_ENTERS) == null)
        {
            eventProperties.Add(
                new EventProperty { key = KEY_ENEMY_ENTERS, value = new UnityEvent() }
            );
        }
    }

    /// <summary>
    /// Get the starting unit for this grid point.
    /// </summary>
    public CharacterInstance GetStartingUnit()
    {
        var prop = unitProperties.Find(p => p.key == KEY_STARTING_UNIT);
        return prop?.value;
    }

    /// <summary>
    /// Set the starting unit for this grid point.
    /// </summary>
    public void SetStartingUnit(CharacterInstance unit)
    {
        var prop = unitProperties.Find(p => p.key == KEY_STARTING_UNIT);
        if (prop != null)
        {
            prop.value = unit;
        }
        else
        {
            unitProperties.Add(new UnitProperty { key = KEY_STARTING_UNIT, value = unit });
        }
    }

    /// <summary>
    /// Get the Friendly Enters event for this grid point.
    /// </summary>
    public UnityEvent GetFriendlyEntersEvent()
    {
        var prop = eventProperties.Find(p => p.key == KEY_FRIENDLY_ENTERS);
        return prop?.value;
    }

    /// <summary>
    /// Get the Enemy Enters event for this grid point.
    /// </summary>
    public UnityEvent GetEnemyEntersEvent()
    {
        var prop = eventProperties.Find(p => p.key == KEY_ENEMY_ENTERS);
        return prop?.value;
    }
}
