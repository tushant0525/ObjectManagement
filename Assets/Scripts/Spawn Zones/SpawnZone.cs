using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnZone : GameLevelObject
{
    [SerializeField, Range(0f, 50f)]
    float spawnSpeed;

    [SerializeField]
    SpawnConfiguration spawnConfig;

    public abstract Vector3 SpawnPoint { get; }

    float spawnProgress;

    public override void GameUpdate()
    {
        spawnProgress += Time.deltaTime * spawnSpeed;
        while (spawnProgress >= 1f)
        {
            spawnProgress -= 1f;
            SpawnShapes();
        }
    }

    public virtual void SpawnShapes()
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        shape.gameObject.layer = gameObject.layer;
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        SetupColor(shape);
        float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
        if (angularSpeed != 0f)
        {
            var rotation = shape.AddBehavior<RotationShapeBehavior>();
            rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
        }
        float speed = spawnConfig.speed.RandomValueInRange;
        if (speed != 0f)
        {
            var movement = shape.AddBehavior<MovementShapeBehavior>();
            movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
        }
        SetupOscillation(shape);

        Vector3 lifecycleDurations = spawnConfig.lifecycle.RandomDurations;

        int satelliteCount = spawnConfig.satellite.amount.RandomValueInRange;
        for (int i = 0; i < satelliteCount; i++)
        {
            CreateSatelliteFor(
                 shape,
                 spawnConfig.satellite.uniformLifecycles ?
                     lifecycleDurations : spawnConfig.lifecycle.RandomDurations
             );
        }
        SetupLifecycle(shape, lifecycleDurations);
    }



    private void SetupOscillation(Shape shape)
    {
        float amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
        float frequency = spawnConfig.oscillationFrequency.RandomValueInRange;
        if (amplitude == 0f || frequency == 0f)
        {
            return;
        }
        var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
        oscillation.Offset = GetDirectionVector(
            spawnConfig.oscillationDirection, shape.transform
        ) * amplitude;
        oscillation.Frequency = frequency;
    }
    private Vector3 GetDirectionVector(SpawnConfiguration.MovementDirection direction, Transform t)
    {
        switch (direction)
        {
            case SpawnConfiguration.MovementDirection.Upward:
                return transform.up;
            case SpawnConfiguration.MovementDirection.Outward:
                return (t.localPosition - transform.position).normalized;
            case SpawnConfiguration.MovementDirection.Random:
                return Random.onUnitSphere;
            default:
                return transform.forward;
        }
    }

    //Focal shape is the shape that the satellite will orbit around
    void CreateSatelliteFor(Shape focalShape, Vector3 lifecycleDurations)
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        shape.gameObject.layer = gameObject.layer;
        Transform t = shape.transform;
        t.localRotation = Random.rotation;
        t.localScale =
              focalShape.transform.localScale *
              spawnConfig.satellite.relativeScale.RandomValueInRange;
        SetupColor(shape);
        shape.AddBehavior<SatelliteShapeBehavior>().Initialize(
            shape, focalShape,
            spawnConfig.satellite.orbitRadius.RandomValueInRange,
            spawnConfig.satellite.orbitFrequency.RandomValueInRange
        );
        SetupLifecycle(shape, lifecycleDurations);
    }
    private void SetupColor(Shape shape)
    {
        if (spawnConfig.uniformColor)
        {
            shape.SetColor(spawnConfig.color.RandomInRange);
        }
        else
        {
            for (int i = 0; i < shape.ColorCount; i++)
            {
                shape.SetColor(spawnConfig.color.RandomInRange, i);
            }
        }
    }
    void SetupLifecycle(Shape shape, Vector3 durations)
    {
        if (durations.x > 0f)
        {
            if (durations.y > 0f || durations.z > 0f)
            {
                shape.AddBehavior<LifecycleShapeBehavior>().Initialize(
                    shape, durations.x, durations.y, durations.z);
            }
            else
            {
                shape.AddBehavior<GrowingShapeBehavior>().Initialize(
                    shape, durations.x);
            }
        }
        else if (durations.y > 0f)
        {
            shape.AddBehavior<LifecycleShapeBehavior>().Initialize(
                shape, durations.x, durations.y, durations.z);
        }
        else if (durations.z > 0f)
        {
            shape.AddBehavior<DyingShapeBehavior>().Initialize(
                shape, durations.z);
        }
    }
    public override void Save(GameDataWriter writer)
    {
        writer.Write(spawnProgress);
    }

    public override void Load(GameDataReader reader)
    {
        spawnProgress = reader.ReadFloat();
    }
}
