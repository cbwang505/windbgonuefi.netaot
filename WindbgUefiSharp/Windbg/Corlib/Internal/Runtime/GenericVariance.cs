namespace Internal.Runtime
{
    internal enum GenericVariance : byte
    {
        NonVariant = 0,
        Covariant = 1,
        Contravariant = 2,
        ArrayCovariant = 0x20
    }
}
