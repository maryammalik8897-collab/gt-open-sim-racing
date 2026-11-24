using Godot;

namespace UniversalOpenSimulator.Scripts;

public partial class Main : Node3D
{
    private RigidBody3D _car = null!;
    private MeshInstance3D _body = null!;
    private Camera3D _camera = null!;
    private Label _hud = null!;

    private float _throttle;
    private float _steering;
    private float _lapStart = -1f;
    private float _lapTime;

    public override void _Ready()
    {
        CreateEnvironment();
        CreateCar();
        CreateHud();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_car == null) return;

        _throttle = Input.GetActionStrength("throttle") - Input.GetActionStrength("brake");
        _steering = Input.GetActionStrength("steer_left") - Input.GetActionStrength("steer_right");

        if (Input.IsActionJustPressed("reset") || _car.GlobalPosition.Y < -10f)
            ResetCar();

        Vector3 forward = -_car.GlobalTransform.Basis.Z;
        Vector3 right = _car.GlobalTransform.Basis.X;
        Vector3 linear = _car.LinearVelocity;

        float forwardSpeed = linear.Dot(forward);
        float lateralSpeed = linear.Dot(right);

        float driveForce = -_throttle * 5200f;
        driveForce = Mathf.Clamp(driveForce, -9000f, 5200f);
        _car.ApplyCentralForce(forward * driveForce);

        _car.ApplyCentralForce(-right * lateralSpeed * 250f);
        _car.ApplyCentralForce(-linear * 18f * Mathf.Sign(linear.Length()));

        float steeringTorque = _steering * 1100f * Mathf.Clamp(forwardSpeed / 8f, -1f, 1f);
        _car.ApplyTorque(Vector3.Up * steeringTorque);

        UpdateLap(delta);
        UpdateCamera(delta);
        UpdateHud(forwardSpeed);
    }

    private void CreateEnvironment()
    {
        Environment environment = new();
        WorldEnvironment world = new();
        environment.BackgroundMode = Environment.BGMode.Sky;
        Sky sky = new();
        ProceduralSkyMaterial skyMaterial = new();
        skyMaterial.SkyTopColor = new Color("5598d8");
        skyMaterial.SkyHorizonColor = new Color("d8e6f2");
        sky.SkyMaterial = skyMaterial;
        environment.Sky = sky;
        environment.AmbientLightColor = new Color("b5c8dc");
        world.Environment = environment;
        AddChild(world);

        DirectionalLight3D sun = new();
        sun.RotationDegrees = new Vector3(-42f, -35f, 0f);
        sun.ShadowEnabled = true;
        AddChild(sun);

        CreatePlane("Grass", new Vector3(0f, -0.22f, 0f), new Vector3(150f, 0.25f, 150f), new Color("3e8538"));
        CreatePlane("Road", new Vector3(0f, -0.08f, 0f), new Vector3(18f, 0.12f, 160f), new Color("35393d"));

        for (int z = -75; z <= 75; z += 6)
        {
            CreatePlane("LaneMarker", new Vector3(-4.5f, 0f, z), new Vector3(0.12f, 0.01f, 3f), new Color("eeeeee"));
            CreatePlane("LaneMarker", new Vector3(4.5f, 0f, z), new Vector3(0.12f, 0.01f, 3f), new Color("eeeeee"));
        }

        for (int i = 0; i < 30; i++)
        {
            float x = i % 2 == 0 ? -28f : 28f;
            float z = -70f + i * 5f;
            CreateTree(x, z);
        }

        // Finish/start gantry
        CreatePlane("Finish", new Vector3(0f, 0.03f, 30f), new Vector3(18f, 0.02f, 0.5f), new Color("eeeeee"));
    }

    private void CreateCar()
    {
        _car = new RigidBody3D
        {
            Mass = 1250f,
            LinearDamp = 0.05f,
            AngularDamp = 0.35f,
            Position = new Vector3(0f, 1.2f, 20f)
        };
        _car.RotationDegrees = new Vector3(0f, 180f, 0f);

        CollisionShape3D shape = new();
        BoxShape3D box = new() { Size = new Vector3(2f, 0.55f, 4.2f) };
        shape.Shape = box;
        shape.Position = Vector3.Up * 0.15f;
        _car.AddChild(shape);

        _body = new MeshInstance3D();
        BoxMesh bodyMesh = new() { Size = new Vector3(2f, 0.55f, 4.2f) };
        _body.Mesh = bodyMesh;
        _body.Position = Vector3.Up * 0.15f;
        StandardMaterial3D paint = new()
        {
            AlbedoColor = new Color("1467d6"),
            Metallic = 0.15f,
            Roughness = 0.38f
        };
        _body.MaterialOverride = paint;
        _car.AddChild(_body);

        MeshInstance3D cabin = new();
        BoxMesh cabinMesh = new() { Size = new Vector3(1.65f, 0.52f, 1.75f) };
        cabin.Mesh = cabinMesh;
        cabin.Position = new Vector3(0f, 0.68f, 0.25f);
        cabin.MaterialOverride = new StandardMaterial3D
        {
            AlbedoColor = new Color("101820"),
            Metallic = 0.1f,
            Roughness = 0.2f
        };
        _car.AddChild(cabin);

        Vector3[] wheelPositions =
        {
            new Vector3(-1.05f, -0.15f, -1.35f), new Vector3(1.05f, -0.15f, -1.35f),
            new Vector3(-1.05f, -0.15f, 1.35f), new Vector3(1.05f, -0.15f, 1.35f)
        };

        foreach (Vector3 position in wheelPositions)
        {
            MeshInstance3D wheel = new();
            CylinderMesh mesh = new()
            {
                TopRadius = 0.38f,
                BottomRadius = 0.38f,
                Height = 0.22f
            };
            wheel.Mesh = mesh;
            wheel.Position = position;
            wheel.RotationDegrees = new Vector3(0f, 0f, 90f);
            wheel.MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color("111111"),
                Roughness = 0.8f
            };
            _car.AddChild(wheel);
        }

        AddChild(_car);

        SpotLight3D headlights = new();
        headlights.Position = new Vector3(0f, 0.35f, -2.1f);
        headlights.RotationDegrees = new Vector3(0f, 180f, 0f);
        headlights.LightEnergy = 5f;
        _car.AddChild(headlights);
    }

    private void CreateHud()
    {
        CanvasLayer canvas = new();
        AddChild(canvas);
        _hud = new Label
        {
            Position = new Vector2(24f, 24f),
            Size = new Vector2(430f, 140f)
        };
        _hud.AddThemeFontSizeOverride("font_size", 24);
        canvas.AddChild(_hud);
    }

    private void UpdateCamera(double delta)
    {
        if (_camera == null)
        {
            _camera = new Camera3D();
            AddChild(_camera);
            _camera.Current = true;
        }

        Vector3 desired = _car.GlobalPosition - _car.GlobalTransform.Basis.Z * 8f + Vector3.Up * 4f;
        _camera.GlobalPosition = _camera.GlobalPosition.Lerp(desired, (float)delta * 5f);
        _camera.LookAt(_car.GlobalPosition + Vector3.Up * 0.8f, Vector3.Up);
    }

    private void UpdateLap(double delta)
    {
        float z = _car.GlobalPosition.Z;
        if (z > 29f && z < 31f)
        {
            if (_lapStart < 0f)
                _lapStart = (float)Time.GetTicksMsec() / 1000f;
        }

        if (_lapStart >= 0f)
        {
            float now = (float)Time.GetTicksMsec() / 1000f;
            _lapTime = now - _lapStart;
        }
    }

    private void UpdateHud(float forwardSpeed)
    {
        float kph = Mathf.Abs(forwardSpeed) * 3.6f;
        _hud.Text = $"UNIVERSAL OPEN SIMULATOR\nSPEED: {kph:000} km/h\nTHROTTLE: {Mathf.Max(0f, _throttle) * 100f:0}%\nLAP: {_lapTime:0.00}s\nW/S accelerate/brake  A/D steer  R reset";
    }

    private void ResetCar()
    {
        _car.GlobalPosition = new Vector3(0f, 1.2f, 20f);
        _car.Rotation = new Vector3(0f, Mathf.Pi, 0f);
        _car.LinearVelocity = Vector3.Zero;
        _car.AngularVelocity = Vector3.Zero;
    }

    private void CreatePlane(string name, Vector3 position, Vector3 size, Color color)
    {
        StaticBody3D ground = new() { Name = name, Position = position };
        CollisionShape3D collision = new();
        BoxShape3D shape = new() { Size = size };
        collision.Shape = shape;
        ground.AddChild(collision);

        MeshInstance3D meshInstance = new();
        BoxMesh mesh = new() { Size = size };
        meshInstance.Mesh = mesh;
        meshInstance.MaterialOverride = new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.9f
        };
        ground.AddChild(meshInstance);
        AddChild(ground);
    }

    private void CreateTree(float x, float z)
    {
        MeshInstance3D trunk = new();
        CylinderMesh trunkMesh = new() { TopRadius = 0.18f, BottomRadius = 0.25f, Height = 2.2f };
        trunk.Mesh = trunkMesh;
        trunk.Position = new Vector3(x, 1.05f, z);
        trunk.MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color("6a4023") };
        AddChild(trunk);

        MeshInstance3D crown = new();
        SphereMesh crownMesh = new() { Radius = 1.35f, Height = 2.7f };
        crown.Mesh = crownMesh;
        crown.Position = new Vector3(x, 3f, z);
        crown.MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color("285c32") };
        AddChild(crown);
    }
}
