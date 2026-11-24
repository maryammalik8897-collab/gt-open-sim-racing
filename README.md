# Universal Open Simulator

A modular, open-source Godot 4 / C# simulator for cars, motorcycles, airplanes, jets, and rockets.

> This repository is an implementation foundation. It does not claim to contain every real-world vehicle or production-grade flight/aerospace physics.

## Included first prototype

- Procedural test runway and ground
- Ground vehicle with throttle, braking, steering, tire slip, suspension, and lap HUD
- Aircraft, jet, motorcycle, and rocket model interfaces
- Pacejka-inspired combined tire model
- Per-wheel temperature and wear
- Newline-delimited UDP telemetry on `127.0.0.1:9871`
- Replaceable raw force-feedback torque API
- Modular JSON-oriented vehicle architecture

## Controls

- **W/S:** throttle/brake
- **A/D:** steer
- **I/K:** pitch interface (aircraft/jet/rocket model)
- **R:** reset

## Run

1. Install Godot 4.3+ with .NET support.
2. Install .NET 8 SDK.
3. Open the repository and press **F5**.

```bash
dotnet build UniversalOpenSimulator.csproj
```

## Layout

```text
physics/       vehicle dynamics and tire models
vehicles/      car, motorcycle, aircraft, jet, rocket implementations
tracks/        procedural and external track assets
telemetry/     UDP telemetry and force-feedback interfaces
ui/            HUD and setup interfaces
mods/          community vehicle and track manifests
docs/          model and protocol documentation
scripts/       scene and runtime systems
```

## Telemetry example

```python
import socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(("127.0.0.1", 9871))
print(sock.recv(65535).decode("utf-8"))
```

## License

MIT. Original code may be freely modified and redistributed.
