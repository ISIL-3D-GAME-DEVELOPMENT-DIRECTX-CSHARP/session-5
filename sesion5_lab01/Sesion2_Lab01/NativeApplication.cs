using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

using Device = SharpDX.Direct3D11.Device;

namespace Sesion2_Lab01 {
    public class NativeApplication {

        private const int App_Width = 800;
        private const int App_Height = 600;

        // Es como el Windows.Forms, pero este se utiliza nativamente para el DirectX
        // Se necesita las librerias:
        //  1. System.Windows.Forms
        //  2. SharpDX.Windows
        private RenderForm mRenderForm;

        private Texture2D mBackBufferFBO;
        private RenderTargetView mRenderTargetView;
        private Device mDevice;
        private DeviceContext mDeviceContext;

        private Factory mFactory;

        private NViewport mViewport;
        private RenderCamera mRenderCamera;
        private Shader3DProgram mShaderProgram;

        private NPrimitiveCube3D mCube3D;

        private SwapChain mSwapChain;
        private SwapChainDescription mSwapChainDescription;

        public NativeApplication() {
            mRenderForm = new RenderForm("Sesion 5::Aplicacion Nativa DirectX 11");

            mSwapChainDescription = new SwapChainDescription(); // es una estructura, no es necesario construirlo
            mSwapChainDescription.BufferCount = 1;
            mSwapChainDescription.IsWindowed = true; // es ventana
            mSwapChainDescription.SwapEffect = SwapEffect.Discard;
            mSwapChainDescription.Usage = Usage.RenderTargetOutput;
            mSwapChainDescription.OutputHandle = mRenderForm.Handle; // le pasamos el puntero de nuestro RenderForm
            mSwapChainDescription.SampleDescription.Count = 1;
            mSwapChainDescription.SampleDescription.Quality = 0;
            mSwapChainDescription.ModeDescription.Width = NativeApplication.App_Width;  // aqui definimos el ancho de la ventana
            mSwapChainDescription.ModeDescription.Height = NativeApplication.App_Height; // aqui definimos el alto de la ventana
            mSwapChainDescription.ModeDescription.RefreshRate.Numerator = 60; // aqui definimos el Frame Rate
            mSwapChainDescription.ModeDescription.RefreshRate.Denominator = 1; // siempre por defecto 1... la matematica es Denominator / Numerator
            mSwapChainDescription.ModeDescription.Format = Format.R8G8B8A8_UNorm; // se define el formato del color de la ventana

            // Creamos la tarjeta grafica usando el SwapChainDescription
            // Esto nos devuelve:
            //  1. Device -> La tarjeta grafica
            //  2. SwapChain -> Control del Frame Rate
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, mSwapChainDescription,
                out mDevice, out mSwapChain);

            // recogemos el contexto de la tarjeta de video
            mDeviceContext = mDevice.ImmediateContext;

            RasterizerStateDescription rsd = new RasterizerStateDescription();
            rsd.CullMode = CullMode.Front;
            rsd.FillMode = FillMode.Solid;
            rsd.IsFrontCounterClockwise = true;
            rsd.DepthBias = 0;
            rsd.DepthBiasClamp = 0;
            rsd.SlopeScaledDepthBias = 0;
            rsd.IsDepthClipEnabled = true;
            rsd.IsMultisampleEnabled = false;

            RasterizerState rs = new RasterizerState(mDevice, rsd);

            // Set the rasterizer state to the D3D Context
            mDeviceContext.Rasterizer.State = rs;

            // recogemos el padre constructor de la ventana
            mFactory = mSwapChain.GetParent<Factory>();
            // ignoramos todos los eventos de la ventana
            mFactory.MakeWindowAssociation(mRenderForm.Handle, WindowAssociationFlags.IgnoreAll);

            // creamos un frame buffer object, y usamos este back buffer para almacenar la data del render
            mBackBufferFBO = Texture2D.FromSwapChain<Texture2D>(mSwapChain, 0);

            // creamos un render target view para manejar el render usando nuestro frame buffer object
            mRenderTargetView = new RenderTargetView(mDevice, mBackBufferFBO);

            // cargamos y construimos nuestro Shader
            mShaderProgram = new Shader3DProgram(mDevice);
            mShaderProgram.Load("Content/Fx_Primitive3D.fx");

            mCube3D = new NPrimitiveCube3D(0, 0, 0, 2f);

            // Ahora inicializamos algunos datos para poder dibujar
            PreConfiguration();

            // Ahora creamos algo muy importante! Nuestro Render Loop, donde ira nuestro Draw y Update!
            RenderLoop.Run(mRenderForm, OnRenderLoop);
        }

        private void PreConfiguration() {
            // preparamos nuestros parametros para dibujar
            // creamos una camara para el escenario
            mViewport = new NViewport(NativeApplication.App_Width, NativeApplication.App_Height);
            mRenderCamera = new RenderCamera(mViewport, -5f);

            // Creamos nuestro Viewport que define el tamanio de nuestras dimension de dibujo para
            // el DirectX
            mDeviceContext.Rasterizer.SetViewports(new Viewport(0, 0, mViewport.Width,
                mViewport.Height, 0.0f, 1.0f));

            // ahora pasamos nuestro Frame Buffer Object
            mDeviceContext.OutputMerger.SetTargets(mRenderTargetView);


        }

        private void OnRenderLoop() {
            Update();
            Draw();
        }

        private void Update() {
            // actualizamos nuestra camara
            mRenderCamera.Update();

            mShaderProgram.RotationX += 0.001f;
            mShaderProgram.RotationZ += 0.001f;

            // mandamos la informacion al Shader
            mShaderProgram.Update(mCube3D.Vertices, mCube3D.Indices);
        }

        private void Draw() {
            // aqui definimos nuestro color usando Color4
            Color4 clearColor = Color4.Black;
            clearColor.Red = 0f;
            clearColor.Green = 1f;
            clearColor.Blue = 0f;

            // aqui limpiamos nuestra ventana con un color solido
            mDeviceContext.ClearRenderTargetView(mRenderTargetView, clearColor);

            /////////////////// EMPEZAMOS A DIBUJAR NUESTRO PRIMITIVO ///////////////
            mShaderProgram.Draw(mRenderCamera.transformed);

            // hacemos un swap de buffers
            mSwapChain.Present(0, PresentFlags.None);
        }
    }
}
