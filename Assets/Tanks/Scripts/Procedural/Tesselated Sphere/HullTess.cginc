 #include "DomainTess.cginc"
 PatchConstantOutput PatchConstantFunction(InputPatch<TessellationControlPoint, 4> inputPatch, uint patchId : SV_PrimitiveID)
{
    PatchConstantOutput output;
    float tessellationFactor = 100.0f;
    output.edges[0] = output.edges[1] = output.edges[2] = output.edges[3] = tessellationFactor;
    output.inside[0] = output.inside[1] = tessellationFactor;
    return output;
}
[UNITY_domain("quad")]
[UNITY_partitioning("integer")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_outputcontrolpoints(4)]
[UNITY_patchconstantfunc("PatchConstantFunction")]
TessellationControlPoint Hul(InputPatch<TessellationControlPoint, 4> inputPatch, uint outputControlPointID : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
return inputPatch[outputControlPointID];
}