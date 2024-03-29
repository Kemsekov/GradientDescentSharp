using MathNet.Numerics.LinearAlgebra.Storage;
namespace GradientDescentSharp.ComplexDataStructures;

class DataAccessVectorStorage : VectorStorage<double>
{
    public override bool IsDense => true;

    public int StartIndex { get; }
    public IDataAccess<double> Data { get; }

    public DataAccessVectorStorage(IDataAccess<double> data, int startIndex, int length) : base(length)
    {
        StartIndex = startIndex;
        Data = data;
    }
    public override double At(int index)
    {
        return Data[StartIndex + index];
    }

    public override void At(int index, double value)
    {
        Data[StartIndex + index] = value;
    }
}

class DataAccessVectorStorageSingle : VectorStorage<float>
{
    public override bool IsDense => true;

    public int StartIndex { get; }
    public IDataAccess<float> Data { get; }

    public DataAccessVectorStorageSingle(IDataAccess<float> data, int startIndex, int length) : base(length)
    {
        StartIndex = startIndex;
        Data = data;
    }
    public override float At(int index)
    {
        return Data[StartIndex + index];
    }

    public override void At(int index, float value)
    {
        Data[StartIndex + index] = value;
    }
}