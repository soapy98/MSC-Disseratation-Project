#ifndef WHITE_NOISE
#define WHITE_NOISE

//to 1d functions
float Random4DTo1D(float4 value, float4 dotDir = float4(12.9898, 78.233, 37.719, 17.4265)){
	float4 smallValue = cos(value);
	float random = dot(smallValue, dotDir);
	random = frac(sin(random) * 143758.5453);
	return random;
}

//get a scalar random value from a 3d value
float Random3DTo1D(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719)){
	//make value smaller to avoid artefacts
	float3 smallValue = cos(value);
	//get scalar value from 3d vector
	float random = dot(smallValue, dotDir);
	//make value more random by making it bigger and then taking the factional part
	random = frac(sin(random) * 143758.5453);
	return random;
}

float Random2DTo1D(float2 value, float2 dotDir = float2(12.9898, 78.233)){
	float2 smallValue = cos(value);
	float random = dot(smallValue, dotDir);
	random = frac(sin(random) * 143758.5453);
	return random;
}

float Rand1DTo1D(float3 value, float mutator = 0.546){
	float random = frac(sin(value + mutator) * 143758.5453);
	return random;
}

//to 2d functions

float2 Random3DTo2D(float3 value){
	return float2(
		Random3DTo1D(value, float3(12.989, 78.233, 37.719)),
		Random3DTo1D(value, float3(39.346, 11.135, 83.155))
	);
}

float2 Random2DTo2D(float2 value){
	return float2(
		Random2DTo1D(value, float2(12.989, 78.233)),
		Random2DTo1D(value, float2(39.346, 11.135))
	);
}

float2 Radom1DTo2D(float value){
	return float2(
		Random2DTo1D(value, 3.9812),
		Random2DTo1D(value, 7.1536)
	);
}

//to 3d functions

float3 rand3dTo3d(float3 value){
	return float3(
		Random3DTo1D(value, float3(12.989, 78.233, 37.719)),
		Random3DTo1D(value, float3(39.346, 11.135, 83.155)),
		Random3DTo1D(value, float3(73.156, 52.235, 09.151))
	);
}

float3 rand2dTo3d(float2 value){
	return float3(
		Random2DTo1D(value, float2(12.989, 78.233)),
		Random2DTo1D(value, float2(39.346, 11.135)),
		Random2DTo1D(value, float2(73.156, 52.235))
	);
}

float3 Random1DTo3D(float value){
	return float3(
		Rand1DTo1D(value, 3.9812),
		Rand1DTo1D(value, 7.1536),
		Rand1DTo1D(value, 5.7241)
	);
}

// to 4d // TEMP
float4 rand4dTo4d(float4 value){
	return float4(
		Random4DTo1D(value, float4(12.989, 78.233, 37.719, -12.15)),
		Random4DTo1D(value, float4(39.346, 11.135, 83.155, -11.44)),
		Random4DTo1D(value, float4(73.156, 52.235, 09.151, 62.463)),
		Random4DTo1D(value, float4(-12.15, 12.235, 41.151, -1.135))
	);
}

#endif