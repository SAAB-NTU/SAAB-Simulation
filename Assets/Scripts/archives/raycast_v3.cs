using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class raycast_v3 : MonoBehaviour
{
    private void RaycasExample()
    {
        // Perform a single raycast using RaycastCommand and wait for it to complete
        // Setup the command and result buffers
        var results = new NativeArray<RaycastHit>(100, Allocator.TempJob);

        var commands = new NativeArray<RaycastCommand>(100, Allocator.TempJob);

        // Set the data of the first command
        Vector3 origin = Vector3.forward * -10;

        Vector3 direction = Vector3.forward;

        for(int i=0;i<100;++i)
        { 
            commands[i] = new RaycastCommand(transform.position, Quaternion.AngleAxis(-90 / 2 + i * (90 / (100 - 1)), Vector3.up)*transform.TransformDirection(Vector3.forward));
        }
        

        // Schedule the batch of raycasts
        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 100, default(JobHandle));
        
        // Wait for the batch processing job to complete
        handle.Complete();

        // Copy the result. If batchedHit.collider is null there was no hit
        for (int i = 0; i < 100; ++i)
        {
            RaycastHit batchedHit = results[i];
            print(results.Length);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-90 / 2 + i * (90 / (100 - 1)), Vector3.up)* transform.TransformDirection(Vector3.forward) * results[i].distance, Color.green);
        }
        
        // Dispose the buffers
        results.Dispose();
        commands.Dispose();
    }
    private void FixedUpdate()
    {
        RaycasExample();
    }
}
    
        
 
    
  


    

