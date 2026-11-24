# Physics Model

The initial implementation uses approximations that are useful for a prototype but are not certification-grade models.

## Ground tires

A Pacejka-inspired function combines tire slip angle and slip ratio. Per-wheel temperature and wear modify available grip. Suspension applies spring and damper force from a ray cast to the ground.

## Aircraft and jets

The aircraft interface will use lift, drag, thrust, and moment coefficients. The jet implementation will add afterburner thrust and engine-state limits.

## Motorcycles

The motorcycle interface will use combined roll, yaw, and tire states. It must not be represented as a four-wheel vehicle with a low wheelbase.

## Rockets

The rocket interface separates atmosphere, thrust, mass, propellant, staging, and guidance. It should use a point-mass or rigid-body integration scheme rather than treating the vehicle as a conventional aircraft.

## Validation goals

- Fixed-step integration
- Repeatable benchmark tests
- Realistic unit tests and telemetry
- No hidden canned effects in core force calculations
