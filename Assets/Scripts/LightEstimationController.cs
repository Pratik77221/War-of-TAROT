using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class LightEstimationController : MonoBehaviour
{
    public ARCameraManager arCameraManager; // Reference to the AR Camera Manager
    public Light directionalLight; // Reference to the directional light
    //public Material objectMaterial; // Material of the 3D object to apply Spherical Harmonics

    private Vector3 initialLightDirection; // Store the initial light direction

    private void Start()
    {
        // Store the initial direction of the directional light
        initialLightDirection = directionalLight.transform.forward;
    }

    private void OnFrameReceived(ARCameraFrameEventArgs args)
    {
        //Shader.SetGlobalFloat("_GlobalLightEstimation", Frame.LightEstimate.PixelIntensity);
        // Check if light estimation data is available
        /*
        if (args.lightEstimation.averageBrightness.HasValue)
        {
            // Update the directional light's intensity
            directionalLight.intensity = args.lightEstimation.averageBrightness.Value;

            // Adjust shadow strength based on light intensity
            float shadowStrength = Mathf.Clamp(args.lightEstimation.averageBrightness.Value, 0.5f, 1f);
            directionalLight.shadowStrength = shadowStrength;
        }*/

        if (args.lightEstimation.averageColorTemperature.HasValue)
        {
            // Update the directional light's color temperature
            directionalLight.colorTemperature = args.lightEstimation.averageColorTemperature.Value;
        }
        /*
        if (args.lightEstimation.colorCorrection.HasValue)
        {
            // Update the directional light's color
            directionalLight.color = args.lightEstimation.colorCorrection.Value;
        }*/

        if (args.lightEstimation.mainLightDirection.HasValue)
        {
            // Update the directional light's rotation based on the estimated light direction
            Vector3 estimatedLightDirection = args.lightEstimation.mainLightDirection.Value;
            Quaternion targetRotation = Quaternion.LookRotation(estimatedLightDirection, Vector3.up);
            directionalLight.transform.rotation = Quaternion.Slerp(directionalLight.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        else
        {
            // Fallback to the initial light direction if no estimation is available
            //directionalLight.transform.rotation = Quaternion.LookRotation(initialLightDirection, Vector3.up);
            Quaternion targetRotation = Quaternion.LookRotation(initialLightDirection, Vector3.up);
            directionalLight.transform.rotation = Quaternion.Slerp(directionalLight.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

         if (args.lightEstimation.mainLightIntensityLumens.HasValue)
        {
             // Update the directional light's intensity
            directionalLight.intensity = Mathf.Clamp(args.lightEstimation.mainLightIntensityLumens.Value, 0.5f, 3.0f);

            // Adjust shadow strength based on light intensity
            float shadowStrength = Mathf.Clamp(args.lightEstimation.mainLightIntensityLumens.Value, 0.5f, 1f);
            directionalLight.shadowStrength = shadowStrength;
        }

        if (args.lightEstimation.mainLightColor.HasValue)
        {
            // Update the directional light's color
            directionalLight.color = args.lightEstimation.mainLightColor.Value;
        }

        // Update ambient lighting using Spherical Harmonics
        if (args.lightEstimation.ambientSphericalHarmonics.HasValue)
        {
            // Apply Spherical Harmonics to the scene's ambient light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientProbe = args.lightEstimation.ambientSphericalHarmonics.Value;

            // Optionally, apply Spherical Harmonics to the object's material
            /*
            if (objectMaterial != null)
            {
                objectMaterial.SetVectorArray("_SHAr", new Vector4[] { args.lightEstimation.ambientSphericalHarmonics.Value[0] });
                objectMaterial.SetVectorArray("_SHAg", new Vector4[] { args.lightEstimation.ambientSphericalHarmonics.Value[1] });
                objectMaterial.SetVectorArray("_SHAb", new Vector4[] { args.lightEstimation.ambientSphericalHarmonics.Value[2] });
            }*/
        }

    }

    private void Update()
    {
        arCameraManager.frameReceived += OnFrameReceived;
        //Shader.SetGlobalFloat("_GlobalLightEstimation", Frame.LightEstimate.PixelIntensity);
    }
}