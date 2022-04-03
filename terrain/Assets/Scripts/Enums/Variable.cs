public enum Variable
{
    //numbers are arbitrary
    //negative = physical
    NoSections = -1, 
    IsTailEnabled = -2, 
    BodyColour = -3, 
    AngleConstraint = -4, 
    TailMassMultiplier = -5,
    Size = -6, 
    Mass = -7,
    Length = -8,
    UniformBody = -9,
    NoLegs = -10,
    AngleOffset = -11,
    //positive = movement
    IsRotating = 1, 
    UseSin = 2, 
    IsDriving = 3, 
    DriveVelocity = 4,
    RotationMultiplier = 5,
    MaintainSerpentine = 6,
    MaintainGait = 7,
    GaitMultiplier = 8
}