using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes how DescriptorSet maps to real resource binding.
    /// This might become a core part of <see cref="Graphics.Effect"/> at some point.
    /// </summary>
    public struct ResourceGroupBufferUploader
    {
        private ResourceGroupBinding[] resourceGroupBindings;

        public void Compile(GraphicsDevice graphicsDevice, EffectDescriptorSetReflection descriptorSetLayouts, EffectBytecode effectBytecode)
        {
            resourceGroupBindings = new ResourceGroupBinding[descriptorSetLayouts.Layouts.Count];
            for (int setIndex = 0; setIndex < descriptorSetLayouts.Layouts.Count; setIndex++)
            {
                var layout = descriptorSetLayouts.Layouts[setIndex].Layout;
                if (layout == null)
                {
                    resourceGroupBindings[setIndex] = new ResourceGroupBinding { ConstantBufferSlot = -1 };
                    continue;
                }

                var resourceGroupBinding = new ResourceGroupBinding();

                for (int resourceIndex = 0; resourceIndex < layout.Entries.Count; resourceIndex++)
                {
                    var layoutEntry = layout.Entries[resourceIndex];

                    if (layoutEntry.Class == EffectParameterClass.ConstantBuffer)
                    {
                        var constantBuffer = effectBytecode.Reflection.ConstantBuffers.First(x => x.Name == layoutEntry.Key.Name);
                        resourceGroupBinding.ConstantBufferSlot = resourceIndex;
                        resourceGroupBinding.ConstantBufferPreallocated = Buffer.Cosntant.New(graphicsDevice, constantBuffer.Size);
                    }
                }

                resourceGroupBindings[setIndex] = resourceGroupBinding;
            }
        }

        public void Apply(CommandList commandList, ResourceGroup[] resourceGroups, int resourceGroupsOffset)
        {
            if (resourceGroupBindings.Length == 0)
                return;

            var resourceGroupBinding = Interop.Pin(ref resourceGroupBindings[0]);
            for (int i = 0; i < resourceGroupBindings.Length; i++, resourceGroupBinding = Interop.IncrementPinned(resourceGroupBinding))
            {
                var resourceGroup = resourceGroups[resourceGroupsOffset + i];

                // Upload cbuffer (if not done yet)
                if (resourceGroupBinding.ConstantBufferSlot != -1 && resourceGroup != null && resourceGroup.ConstantBuffer.Data != IntPtr.Zero)
                {
                    var preallocatedBuffer = resourceGroup.ConstantBuffer.Buffer;
                    bool needUpdate = true;
                    if (preallocatedBuffer == null)
                        preallocatedBuffer = resourceGroupBinding.ConstantBufferPreallocated; // If it's preallocated buffer, we always upload
                    else if (resourceGroup.ConstantBuffer.Uploaded)
                        needUpdate = false; // If it's not preallocated and already uploaded, we can avoid uploading it again
                    else
                        resourceGroup.ConstantBuffer.Uploaded = true; // First time it is uploaded

                    if (needUpdate)
                    {
                        var mappedConstantBuffer = commandList.MapSubresource(preallocatedBuffer, 0, MapMode.WriteDiscard);
                        Utilities.CopyMemory(mappedConstantBuffer.DataBox.DataPointer, resourceGroup.ConstantBuffer.Data, resourceGroup.ConstantBuffer.Size);
                        commandList.UnmapSubresource(mappedConstantBuffer);
                    }

                    resourceGroup.DescriptorSet.SetConstantBuffer(resourceGroupBinding.ConstantBufferSlot, preallocatedBuffer, 0, resourceGroup.ConstantBuffer.Size);
                }
            }
        }

        internal struct ResourceGroupBinding
        {
            // Constant buffer
            public int ConstantBufferSlot;
            public Buffer ConstantBufferPreallocated;
        }
    }
}