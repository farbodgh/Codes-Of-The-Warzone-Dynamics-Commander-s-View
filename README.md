## AirDefenceSystem.cs
Should be attached to the device managing all anti-air systems. This script coordinates defensive responses and target prioritization.

## ABMMissileLauncher.cs
Manages ABM missile launchers with a custom object pool to enhance performance efficiency.

## ABM.cs
Controls anti-ballistic missiles (ABMs) designed to intercept incoming ballistic missiles and airplanes. Implements rigid body physics for realism and uses lerping in the final phase to move accurately toward its target. Final version incorporates a probability factor to adjust missile reliability.

## CIWS.cs
Controls CIWS turret behavior, ensuring accurate target aiming. Works in tandem with BulletCIWS.cs to predict and aim at potential hit points based on target velocity.

## BulletCIWS.cs
Defines the behavior of CIWS (Close-In Weapon System) turret-fired bullets, designed to counteract incoming cruise missiles and helicopters. Bullets use Unity's physics with gravity enabled.

## BallisticMissile.cs
Controls ballistic missiles, utilizing lerping in the final phase for precision targeting, with other phases relying on Unity's physics engine.

## CruiseMissile.cs
Governs cruise missile behavior, featuring low-altitude flight with constant velocity, facilitating CIWS target prediction and engagement.

## Helicopter.cs ( I only fine tune the sound effect logics of it)
Manages helicopter behavior and sound logic, ensuring cohesive aerial behavior.

## HeloWeaponSystem.cs
Manages helicopter weapons, adjusting aim based on mouse X and Y axis input for machine gun direction.

## HeloMiniGunBullet.cs
Controls helicopter bullet behavior, aiming for realistic physics using Unity's engine.

## MissileControllingSystem.cs
For prototype use: Controls player-fired missiles, directing them towards designated targets. The script is set to undergo significant changes.

## Radar.cs
Handles radar system operations, utilizing a large trigger collider to detect and categorize objects, which are then relayed to the AirDefenseSystem for action.


