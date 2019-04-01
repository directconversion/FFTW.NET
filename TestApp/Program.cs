using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using FFTW.NET;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            /// Make sure either libfftw3-3-x86.dll or libfftw3-3-x64.dll
            /// (according to <see cref="Environment.Is64BitProcess"/>)
            /// is in the output directory

            DFT.Wisdom.Import("wisdom.txt");

            Example1D();

            Console.ReadKey();
            Console.Clear();

            Example2D();

            Console.ReadKey();
            Console.Clear();

            ExampleR2C();

            Console.ReadKey();
            Console.Clear();

            ExampleUsePlanDirectly();
            ExampleUsePlanDirectly1();

            ExampleUsePlanDirectly2d();

            DFT.Wisdom.Export("wisdom.txt");
            Console.WriteLine(DFT.Wisdom.Current);
            Console.ReadKey();
        }

        static void Example1D()
        {
            Complex[] input = new Complex[2048];
            Complex[] output = new Complex[input.Length];

            for (int i = 0; i < input.Length; i++)
                input[i] = Math.Sin(i * 2 * Math.PI * 128 / input.Length);

            using (var pinIn = new PinnedArray<Complex>(input))
            using (var pinOut = new PinnedArray<Complex>(output))
            {
                DFT.FFT(pinIn, pinOut);
                DFT.IFFT(pinOut, pinOut);
            }

            for (int i = 0; i < input.Length; i++)
                Console.WriteLine(output[i] / input[i]);
        }

        static void Example2D()
        {
            using (var input = new AlignedArrayComplex(16, 64, 16))
            using (var output = new AlignedArrayComplex(16, input.GetSize()))
            {
                for (int row = 0; row < input.GetLength(0); row++)
                {
                    for (int col = 0; col < input.GetLength(1); col++)
                        input[row, col] = (double)row * col / input.Length;
                }

                DFT.FFT(input, output);
                DFT.IFFT(output, output);

                for (int row = 0; row < input.GetLength(0); row++)
                {
                    for (int col = 0; col < input.GetLength(1); col++)
                        Console.Write((output[row, col].Real / input[row, col].Real / input.Length).ToString("F2").PadLeft(6));
                    Console.WriteLine();
                }
            }
        }

        static void ExampleR2C()
        {
            double[] input = new double[97];

            var rand = new Random();

            for (int i = 0; i < input.Length; i++)
                input[i] = rand.NextDouble();

            using (var pinIn = new PinnedArray<double>(input))
            using (var output = new FftwArrayComplex(DFT.GetComplexBufferSize(pinIn.GetSize())))
            {
                DFT.FFT(pinIn, output);
                for (int i = 0; i < output.Length; i++)
                    Console.WriteLine(output[i]);
            }
        }
        static void ExampleR2R()
        {
            Double[] input = new Double[97];

            var rand = new Random();

            for (int i = 0; i < input.Length; i++)
                input[i] = (float)rand.NextDouble();

            using (var pinIn = new PinnedArray<Double>(input))
            using (var output = new FftwArrayComplex(DFT.GetComplexBufferSize(pinIn.GetSize())))
            {
                DFT.FFT(pinIn, output);
                for (int i = 0; i < output.Length; i++)
                    Console.WriteLine(output[i]);
            }
        }

        static void ExampleUsePlanDirectly()
        {
            // Use the same arrays for as many transformations as you like.
            // If you can use the same arrays for your transformations, this is faster than calling DFT.FFT / DFT.IFFT
            using (var timeDomain = new FftwArrayComplex(253))
            using (var frequencyDomain = new FftwArrayComplex(timeDomain.GetSize()))
            using (var fft = FftwPlanC2C.Create(timeDomain, frequencyDomain, DftDirection.Forwards))
            using (var ifft = FftwPlanC2C.Create(frequencyDomain, timeDomain, DftDirection.Backwards))
            {
                // Set the input after the plan was created as the input may be overwritten
                // during planning
                for (int i = 0; i < timeDomain.Length; i++)
                    timeDomain[i] = i % 10;

                // timeDomain -> frequencyDomain
                fft.Execute();

                for (int i = frequencyDomain.Length / 2; i < frequencyDomain.Length; i++)
                    frequencyDomain[i] = 0;

                // frequencyDomain -> timeDomain
                ifft.Execute();

                // Do as many forwards and backwards transformations here as you like
            }
        }
        static void ExampleUsePlanDirectly1()
        {
            // Use the same arrays for as many transformations as you like.
            // If you can use the same arrays for your transformations, this is faster than calling DFT.FFT / DFT.IFFT
            using (var timeDomain = new FftwArrayDouble(256))
            using (var frequencyDomain = new FftwArrayDouble(timeDomain.GetSize()))
            using (var fft = FftwPlanR2R.Create(timeDomain, frequencyDomain, DftDirection.Forwards))
            using (var ifft = FftwPlanR2R.Create(frequencyDomain, timeDomain, DftDirection.Backwards))
            {
                // Set the input after the plan was created as the input may be overwritten
                // during planning
                for (int i = 0; i < timeDomain.Length; i++)
                    timeDomain[i] = i % 16;

                // timeDomain -> frequencyDomain
                fft.Execute();

                for (int i = 0; i < frequencyDomain.Length; i++)
                    Console.WriteLine(frequencyDomain[i]);

                //for (int i = frequencyDomain.Length / 2; i < frequencyDomain.Length; i++)
                //    frequencyDomain[i] = 0;
                Console.ReadKey();
                Console.Clear();
                // frequencyDomain -> timeDomain
                ifft.Execute();

                for (int i = 0; i < frequencyDomain.Length; i++)
                    Console.WriteLine(timeDomain[i]);

                Console.ReadKey();

                // Do as many forwards and backwards transformations here as you like
            }
        }

        static void ExampleUsePlanDirectly2d()
        {
            Console.Clear();
            // Use the same arrays for as many transformations as you like.
            // If you can use the same arrays for your transformations, this is faster than calling DFT.FFT / DFT.IFFT
            using (var input = new AlignedArrayDouble(16, 64, 16))
            using (var output = new AlignedArrayDouble(16, input.GetSize()))
            {

                for (int row = 0; row < input.GetLength(0); row++)
                {
                    for (int col = 0; col < input.GetLength(1); col++)
                        input[row, col] = 10;// (double)row * col / input.Length;
                }

                //using (var timeDomain = new FftwArrayDouble(256))
                //using (var frequencyDomain = new FftwArrayDouble(timeDomain.GetSize()))
                using (var fft = FftwPlanR2R.Create(input, output, DftDirection.Forwards))
                using (var ifft = FftwPlanR2R.Create(output, input, DftDirection.Backwards))
                {
                    // Set the input after the plan was created as the input may be overwritten
                    // during planning
                    for (int row = 0; row < input.GetLength(0); row++)
                    {
                        for (int col = 0; col < input.GetLength(1); col++)
                            input[row, col] = col+row;// (double)row * col / input.Length;
                    }

                    // timeDomain -> frequencyDomain
                    fft.Execute();

                    //for (int i = 0; i < frequencyDomain.Length; i++)
                    //    Console.WriteLine(frequencyDomain[i]);

                    //for (int i = frequencyDomain.Length / 2; i < frequencyDomain.Length; i++)
                    //    frequencyDomain[i] = 0;
                    // frequencyDomain -> timeDomain
                    ifft.Execute();

                    for (int row = 0; row < input.GetLength(0); row++)
                    {
                        for (int col = 0; col < input.GetLength(1); col++)
                            Console.Write((input[row, col] / (4*input.Length)).ToString("F1").PadLeft(8));// (output[row, col] / input[row, col] / input.Length).ToString("F2").PadLeft(6));
                        //Console.Write(output[row, col].ToString("F2").PadLeft(10));
                        Console.WriteLine();
                    }
                    //for (int i = 0; i < frequencyDomain.Length; i++)
                    //    Console.WriteLine(timeDomain[i]);

                    Console.ReadKey();

                    // Do as many forwards and backwards transformations here as you like
                }
            }
        }
    }
}
