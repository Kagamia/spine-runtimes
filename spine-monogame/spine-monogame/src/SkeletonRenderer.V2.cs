/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Spine
{
    public partial class SkeletonRenderer
    {
        private BlendState blendStateNormal;
        private BlendState blendStateNormalPMA;
        private BlendState blendStateAdditive;
        private BlendState blendStateAdditivePMA;
        private BlendState blendStateMultiply;
        private BlendState blendStateScreen;
        private int[] quadTrianglesV2 = { 0, 1, 2, 1, 3, 2 };

        private void v2ctor()
        {
            V2.Bone.yDown = true;

            blendStateNormal = this.device.GraphicsProfile == GraphicsProfile.HiDef ?
                Util.CreateBlend_NonPremultipled_Hidef() : BlendState.NonPremultiplied;
            blendStateNormalPMA = BlendState.AlphaBlend;
            blendStateAdditive = BlendState.Additive;
            blendStateAdditivePMA = new BlendState()
            {
                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            };
            blendStateMultiply = new BlendState()
            {
                ColorSourceBlend = Blend.DestinationColor,
                AlphaSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha
            };
            blendStateScreen = new BlendState()
            {
                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.InverseSourceColor,
                AlphaDestinationBlend = Blend.InverseSourceColor
            };
        }

        public void Draw(V2.Skeleton skeleton)
        {
            float[] vertices = this.vertices;
            V2.ExposedList<V2.Slot> drawOrder = skeleton.DrawOrder;
            float skeletonR = skeleton.R, skeletonG = skeleton.G, skeletonB = skeleton.B, skeletonA = skeleton.A;

            Color darkColor = new Color();
            darkColor.A = premultipliedAlpha ? (byte)255 : (byte)0;

            for (int i = 0, n = drawOrder.Count; i < n; i++)
            {
                V2.Slot slot = drawOrder.Items[i];
                V2.Attachment attachment = slot.Attachment;
                BlendState blend = GetState(slot.Data.BlendMode);
                if (device.BlendState != blend)
                {
                    End();
                    device.BlendState = blend;
                }

                if (attachment is V2.RegionAttachment regionAttachment)
                {
                    MeshItem item = batcher.NextItem(4, 6);
                    Array.Copy(quadTrianglesV2, item.triangles, quadTrianglesV2.Length);
                    VertexPositionColorTextureColor[] itemVertices = item.vertices;

                    V2.AtlasRegion region = (V2.AtlasRegion)regionAttachment.RendererObject;
                    item.texture = (Texture2D)region.page.rendererObject;

                    Color color;
                    float a = skeletonA * slot.A * regionAttachment.A;
                    if (premultipliedAlpha)
                    {
                        color = new Color(
                                skeletonR * slot.R * regionAttachment.R * a,
                                skeletonG * slot.G * regionAttachment.G * a,
                                skeletonB * slot.B * regionAttachment.B * a, a);
                    }
                    else
                    {
                        color = new Color(
                                skeletonR * slot.R * regionAttachment.R,
                                skeletonG * slot.G * regionAttachment.G,
                                skeletonB * slot.B * regionAttachment.B, a);
                    }
                    itemVertices[TL].Color = color;
                    itemVertices[BL].Color = color;
                    itemVertices[BR].Color = color;
                    itemVertices[TR].Color = color;

                    regionAttachment.ComputeWorldVertices(slot.Bone, vertices);
                    itemVertices[TL].Position.X = vertices[V2.RegionAttachment.X1];
                    itemVertices[TL].Position.Y = vertices[V2.RegionAttachment.Y1];
                    itemVertices[TL].Position.Z = 0;
                    itemVertices[BL].Position.X = vertices[V2.RegionAttachment.X2];
                    itemVertices[BL].Position.Y = vertices[V2.RegionAttachment.Y2];
                    itemVertices[BL].Position.Z = 0;
                    itemVertices[BR].Position.X = vertices[V2.RegionAttachment.X3];
                    itemVertices[BR].Position.Y = vertices[V2.RegionAttachment.Y3];
                    itemVertices[BR].Position.Z = 0;
                    itemVertices[TR].Position.X = vertices[V2.RegionAttachment.X4];
                    itemVertices[TR].Position.Y = vertices[V2.RegionAttachment.Y4];
                    itemVertices[TR].Position.Z = 0;

                    float[] uvs = regionAttachment.UVs;
                    itemVertices[TL].TextureCoordinate.X = uvs[V2.RegionAttachment.X1];
                    itemVertices[TL].TextureCoordinate.Y = uvs[V2.RegionAttachment.Y1];
                    itemVertices[BL].TextureCoordinate.X = uvs[V2.RegionAttachment.X2];
                    itemVertices[BL].TextureCoordinate.Y = uvs[V2.RegionAttachment.Y2];
                    itemVertices[BR].TextureCoordinate.X = uvs[V2.RegionAttachment.X3];
                    itemVertices[BR].TextureCoordinate.Y = uvs[V2.RegionAttachment.Y3];
                    itemVertices[TR].TextureCoordinate.X = uvs[V2.RegionAttachment.X4];
                    itemVertices[TR].TextureCoordinate.Y = uvs[V2.RegionAttachment.Y4];

                    itemVertices[TL].Color2 = darkColor;
                    itemVertices[BL].Color2 = darkColor;
                    itemVertices[BR].Color2 = darkColor;
                    itemVertices[TR].Color2 = darkColor;
                }
                else if (attachment is V2.MeshAttachment mesh)
                {
                    int vertexCount = mesh.Vertices.Length;
                    if (vertices.Length < vertexCount) vertices = new float[vertexCount];
                    mesh.ComputeWorldVertices(slot, vertices);

                    int[] triangles = mesh.Triangles;
                    MeshItem item = batcher.NextItem(vertexCount >> 1, triangles.Length);
                    Array.Copy(triangles, item.triangles, triangles.Length);

                    V2.AtlasRegion region = (V2.AtlasRegion)mesh.RendererObject;
                    item.texture = (Texture2D)region.page.rendererObject;

                    Color color;
                    float a = skeletonA * slot.A * mesh.A;
                    if (premultipliedAlpha)
                    {
                        color = new Color(
                                skeletonR * slot.R * mesh.R * a,
                                skeletonG * slot.G * mesh.G * a,
                                skeletonB * slot.B * mesh.B * a, a);
                    }
                    else
                    {
                        color = new Color(
                                skeletonR * slot.R * mesh.R,
                                skeletonG * slot.G * mesh.G,
                                skeletonB * slot.B * mesh.B, a);
                    }

                    float[] uvs = mesh.UVs;
                    VertexPositionColorTextureColor[] itemVertices = item.vertices;
                    for (int ii = 0, v = 0; v < vertexCount; ii++, v += 2)
                    {
                        itemVertices[ii].Color = color;
                        itemVertices[ii].Position.X = vertices[v];
                        itemVertices[ii].Position.Y = vertices[v + 1];
                        itemVertices[ii].Position.Z = 0;
                        itemVertices[ii].TextureCoordinate.X = uvs[v];
                        itemVertices[ii].TextureCoordinate.Y = uvs[v + 1];
                        itemVertices[ii].Color2 = darkColor;
                    }
                }
                else if (attachment is V2.SkinnedMeshAttachment skinnedMesh)
                {
                    int vertexCount = skinnedMesh.UVs.Length;
                    if (vertices.Length < vertexCount) vertices = new float[vertexCount];
                    skinnedMesh.ComputeWorldVertices(slot, vertices);

                    int[] triangles = skinnedMesh.Triangles;
                    MeshItem item = batcher.NextItem(vertexCount >> 1, triangles.Length);
                    Array.Copy(triangles, item.triangles, triangles.Length);

                    V2.AtlasRegion region = (V2.AtlasRegion)skinnedMesh.RendererObject;
                    item.texture = (Texture2D)region.page.rendererObject;

                    Color color;
                    float a = skeletonA * slot.A * skinnedMesh.A;
                    if (premultipliedAlpha)
                    {
                        color = new Color(
                                skeletonR * slot.R * skinnedMesh.R * a,
                                skeletonG * slot.G * skinnedMesh.G * a,
                                skeletonB * slot.B * skinnedMesh.B * a, a);
                    }
                    else
                    {
                        color = new Color(
                                skeletonR * slot.R * skinnedMesh.R,
                                skeletonG * slot.G * skinnedMesh.G,
                                skeletonB * slot.B * skinnedMesh.B, a);
                    }

                    float[] uvs = skinnedMesh.UVs;
                    VertexPositionColorTextureColor[] itemVertices = item.vertices;
                    for (int ii = 0, v = 0; v < vertexCount; ii++, v += 2)
                    {
                        itemVertices[ii].Color = color;
                        itemVertices[ii].Position.X = vertices[v];
                        itemVertices[ii].Position.Y = vertices[v + 1];
                        itemVertices[ii].Position.Z = 0;
                        itemVertices[ii].TextureCoordinate.X = uvs[v];
                        itemVertices[ii].TextureCoordinate.Y = uvs[v + 1];
                        itemVertices[ii].Color2 = darkColor;
                    }
                }
            }
        }

        private BlendState GetState(V2.BlendMode blendMode)
        {
            switch (blendMode)
            {
                default:
                case V2.BlendMode.normal: return this.PremultipliedAlpha ? this.blendStateNormalPMA : this.blendStateNormal;
                case V2.BlendMode.additive: return this.PremultipliedAlpha ? this.blendStateAdditivePMA : this.blendStateAdditive;
                case V2.BlendMode.multiply: return this.blendStateMultiply;
                case V2.BlendMode.screen: return this.blendStateScreen;
            }
        }
    }
}
