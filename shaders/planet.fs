/*
 * WebGL core teaching framwork 
 * (C)opyright Hartmut Schirmacher, hschirmacher.beuth-hochschule.de 
 *
 * Fragment Shader: phong
 *
 * expects position and normal vectors in eye coordinates per vertex;
 * expects uniforms for ambient light, directional light, and phong material.
 * 
 *
 */

precision mediump float;

// position and normal in eye coordinates
varying vec4  ecPosition;
varying vec3  ecNormal;

varying vec2 vertexTexCoords2;

// transformation matrices
uniform mat4  modelViewMatrix;
uniform mat4  projectionMatrix;

// Ambient Light
uniform vec3 ambientLight;

// dayLight and Night texture
uniform sampler2D daylightTexture;
uniform sampler2D nightTexture;
uniform sampler2D baryTexture;
uniform sampler2D cloudTexture;

uniform bool dayTime;
uniform bool nightTime;
uniform bool redGreen;
uniform bool glossMap;
uniform bool cloudMode;

// Debug Mode
uniform bool debugMode;


// Material Type
struct PhongMaterial {
    vec3  ambient;
    vec3  diffuse;
    vec3  specular;
    float shininess;
};
// uniform variable for the currently active PhongMaterial
uniform PhongMaterial material;

// Light Source Data for a directional light (not point light)
struct LightSource {

    int  type;
    vec3 direction;
    vec3 color;
    bool on;
    
} ;
uniform LightSource light;

/*

 Calculate surface color based on Phong illumination model.
 - pos:  position of point on surface, in eye coordinates
 - n:    surface normal at pos
 - v:    direction pointing towards the viewer, in eye coordinates
 + assuming directional light
 
 */
vec3 phong(vec3 pos, vec3 n, vec3 v, LightSource light, PhongMaterial material) {
	
    // vector from light to current point
    vec3 l = normalize(light.direction);
    
    // cosine of angle between light and surface normal. 
    float ndotl = dot(n,-l);
 
 	// Color green for vector
    vec3 colorGreen = vec3(0.0, 1.0, 0.0);
    vec3 colorRed = vec3(1.0, 0.0, 0.0);
    
    
    // ambient part, this is a constant term shown on the
    // all sides of the object
    vec3 ambient = material.ambient * ambientLight;
     
     
    vec3 cloud = texture2D (cloudTexture, vertexTexCoords2.st).rgb; 
    
    float cloud2 = texture2D (cloudTexture, vertexTexCoords2.st).r; 
    
    cloud = cloud*cloud2;
    

	if (!cloudMode) {
    	cloud= vec3(0.0,0.0,0.0);

    } 
    
    
    
      
    // Between Night and Day show in DebugMode Green Line
    if (debugMode && ndotl>0.0 && ndotl<= 0.052335956243) 
	return colorGreen;
    

    
    // Get Map Height show ocean blue and land red
    vec3 mapHeight = texture2D (baryTexture, vertexTexCoords2.st).rgb; 
 	if (redGreen) {
  	 if (mapHeight == vec3(0.0, 0.0, 0.0)) {
      	 return colorGreen;
      	 } else
         return colorRed;
	}
    

    // is the current fragment's normal pointing away from the light?
    // then we are on the "back" side of the object, as seen from the light
    if(!nightTime && ndotl<=0.0)
        return ambient;
        
    vec3 day = texture2D (daylightTexture, vertexTexCoords2.st).rgb;
    vec3 night = texture2D (nightTexture, vertexTexCoords2.st).rgb; 
   
    // diffuse contribution
    vec3 diffuseCoeff = material.diffuse;
    if (dayTime && ndotl >= 0.0) {

    	diffuseCoeff = day;
	
    }	
   	if (nightTime && ndotl <=0.0) {
    	return night-cloud;
   	
   	}


   	if (nightTime && dayTime && ndotl > 0.0 ){

    	ambient = night-cloud;
   		diffuseCoeff = day;
   		ambient = (1.0-ndotl)*(1.0-ndotl)*ambient;
   	}
   	
   	
   	if (nightTime && ndotl > 0.0){

    	ambient = night-(cloud*1.5);
   		ambient = (1.0-ndotl)*(1.0-ndotl)*ambient;
   	}	
   		
    	
	vec3 diffuse = diffuseCoeff * light.color * ndotl;

     // reflected light direction = perfect reflection direction
    vec3 r = reflect(l,n);
    
    

    
  
    
    // cosine of angle between reflection dir and viewing dir
    float rdotv = max( dot(r,v), 0.0);
    
    // specular contribution
    
    
    vec3 specularCoeff = material.specular;
    
    float shininess = material.shininess;
    
    if (glossMap) {
  		if (mapHeight == vec3(0.0, 0.0, 0.0)) {
      	 	specularCoeff = material.specular;
      	} else
        specularCoeff = material.specular*2.0;
	}    
	 
    
    
    vec3 specular = specularCoeff * light.color * pow(rdotv, shininess);
 
    
	
	
    
    // return sum of all contributions
    return ambient + diffuse + specular + cloud;
    
}



void main() {
    
    // normalize normal after projection
    vec3 normalEC = normalize(ecNormal);
    
    // do we use a perspective or an orthogonal projection matrix?
    bool usePerspective = projectionMatrix[2][3] != 0.0;
    
    // for perspective mode, the viewing direction (in eye coords) points
    // from the vertex to the origin (0,0,0) --> use -ecPosition as direction.
    // for orthogonal mode, the viewing direction is simply (0,0,1)
    vec3 viewdirEC = usePerspective? normalize(-ecPosition.xyz) : vec3(0,0,1);
    
    // calculate color using phong illumination
    vec3 color = phong( ecPosition.xyz, normalEC, viewdirEC,
                        light, material );
    
    // set fragment color
    
    
    	// TexCoords
	if (debugMode) {
 		if (mod(vertexTexCoords2.x, 0.05) >= 0.025) 
			color = color*0.7;
		else 
			color;
    }
    

    	
    
    gl_FragColor = vec4(color, 1.0);
}
