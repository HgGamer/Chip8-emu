using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Chip8_emu
{
    public class Emulator
    {
     
        public bool[] framebuffer = new bool[64 * 32];
        private byte[] memory = new byte[4096];
        private int PC = 512;
        private int SP = 0;
        private int I;
        private byte[] VN = new byte[16];
        private int[] STACK = new int[32];
        private bool[] keys = new bool[16];
        private byte delaytimer = 0;
        private byte soundtimer = 0;
       
        public void SetKey(byte keycode, bool value)
        {
            keys[keycode] = value;
        }
        public void Tick()
        {
            if (soundtimer > 0)
            {
                soundtimer--;
            }
            if (delaytimer > 0)
            {
                delaytimer--;
            }
            // System.Diagnostics.Debug.WriteLine(getOpCode()+"|PC:"+ PC.ToString("X4") + "|I:" + I + "|v0:" + VN[0] + "|v1:" + VN[1] + "|v2:" + VN[2] + "|v3:" + VN[3] + "|v4:" + VN[4]);

            byte high = memory[PC];
            byte low = memory[PC+1];
            
            byte o1 = (byte)(memory[PC] >> 4);
            byte o2 = (byte)(memory[PC] & 0xf);
            byte o3 = (byte)(memory[PC+1] >> 4);
            byte o4 = (byte)(memory[PC+1] & 0xf);
            int nnn = (o2 * 256 + low);
            switch (o1)
            {
                case 0x0:
                    if(low == 0xE0) //clearscreen
                    {
                        clearScreen();
                        break;
                    }
                    if(low == 0xEE) //RETURN
                    {
                        SP--;
                        PC = STACK[SP];
                        return;
                    }
                    break;
                case 0x1: //jump to NNN
                    PC = nnn;
                    return;
                case 0x2: //call NNN
                    STACK[SP] = PC + 2;
                    SP++;
                    PC = nnn;
                    return;
                case 0x3:
                    if (VN[o2] == low)
                    {
                        PC += 4;
                        return;
                    }
                    PC += 2;
                    return;
                case 0x4: // if (Vx != NN)	Skips the next instruction if VX does not equal NN. (Usually the next instruction is a jump to skip a code block);
                    if (VN[o2] != low)
                    {
                        PC += 4;
                        return;
                    }
                    PC += 2;
                    return;
                case 0x5:
                    if (VN[o2] == VN[o3])
                    {
                        PC += 4;
                        return;
                    }
                    PC += 2;
                    return;
                case 0x6: // Vx = N
                    VN[o2] = low;
                    break;
                case 0x7: // Vx += N
                    VN[o2] += low;
                    break;
                case 0x8:
                    switch (o4)
                    {
                        case 0x0:
                            VN[o2] = VN[o3];
                            break;
                        case 0x1:
                            VN[o2] = (byte)(VN[o2] | VN[o3]);
                            break;
                        case 0x2:
                            VN[o2] = (byte)(VN[o2] & VN[o3]);
                            break;
                        case 0x3:
                            VN[o2] = (byte)(VN[o2] ^ VN[o3]);
                            break;
                        case 0x4:
                            VN[0xf] = 0;
                            if (VN[o2] + VN[o3] > 0xff)
                            {
                                VN[0xf] = 1;
                            }
                            VN[o2] += VN[o3];
                            break;
                        case 0x5:
                            VN[0xf] = 0;
                            if (VN[o2] - VN[o3]  < 0)
                            {
                                VN[0xf] = 1;
                            }
                            VN[o2] -= VN[o3];
                            break;
                        case 0x6:
                            VN[0xf] = (byte)(VN[o2] & 0x1);
                            VN[o2] = (byte)(VN[o2] >> 1);
                            break;
                        case 0x7:
                            VN[0xf] = 0;
                            if (VN[o3] - VN[o2] < 0)
                            {
                                VN[0xf] = 1;
                            }
                            VN[o2] = (byte)(VN[o3] - VN[o2]);
                            break;
                        case 0xE:
                            VN[0xF] = (byte)((VN[o2] >> 7) & 0x1);
                            VN[o2] = (byte)(VN[o2] << 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case 0x9: // if (Vx != Vy)	Skips the next instruction if VX does not equal VY
                    if (VN[o2] != VN[o3])
                    {
                        PC += 4;
                        return;
                    }
                    PC += 2;
                    return;
                case 0xA: //I = NNN
                    I = nnn;
                    break;
                case 0xB:
                    PC = VN[0] + nnn;
                    return;
                case 0xC:
                    var rand = new Random();
                    VN[o2] = (byte)(rand.Next(0, 255) & low); 
                  
                    break;
                case 0xD://	Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N+1 pixels. Each row of 8 pixels is read as bit-coded starting from memory location I; I value does not change after the execution of this instruction. As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that does not happen
                    draw_sprite(VN[o2], VN[o3], o4);
                    break;
                case 0xE:
                    if(o4 == 0xE)
                    {
                        if (keys[o2]) {
                            PC += 4;
                            return;
                        }
                        PC += 2;
                        return;
                    }
                    if (o4 == 0x1)
                    {
                        if (!keys[o2]) {
                            PC += 4;
                            return;
                        }
                        PC += 2;
                        return;
                    }
                    
                    break;
                case 0xF:
                    switch (low)
                    {
                        case 0x07:
                            VN[o2] = delaytimer;
                            break;
                        case 0x0A:
                            for (int i = 0; i < keys.Length; i++)
                            {
                                if (keys[i])
                                {
                                    VN[o2] = (byte)i;
                                    PC += 2;
                                }
                            }
                            return;
                        case 0x15:
                            delaytimer = VN[o2];
                            break;
                        case 0x18:
                            soundtimer = VN[o2];
                            break;
                        case 0x1E:
                            I += VN[o2];
                            break;
                        case 0x29:
                            I = VN[o2] * 5;
                            break;
                        case 0x33:
                            memory[I] = (byte)((VN[o2] % 1000) / 100); // hundred's digit
                            memory[I + 1] = (byte)((VN[o2] % 100) / 10);   // ten's digit
                            memory[I + 2] = (byte)(VN[o2] % 10);         // one's digit
                            break;
                        case 0x55:
                            for (int i = 0; i <= o2; i++)
                            {
                                memory[I + i] = VN[i];
                            }
                            I += o2 + 1;
                            break;
                        case 0x65:
                            for (int i = 0; i <= o2; i++)
                            {
                                VN[i] = memory[I + i];
                            }
                            I += o2 + 1;
                            break;
                        default:
                            break;
                    }
                    
                    break;
                default:
                    break;
            }
           
            PC+=2;
        }
        void draw_sprite(int col, int row, int n)
        {
            
            int byte_index;
            int bit_index;

            // set the collision flag to 0
            VN[0xF] = 0;
            for (byte_index = 0; byte_index < n; byte_index++)
            {
                int line = memory[I + byte_index];
                //System.Diagnostics.Debug.WriteLine(""+line+"|x:"+col+"|y:"+row);
                for (bit_index = 0; bit_index <8; bit_index++)
                {
                    // the value of the bit in the sprite
                    
                    bool bit = ((1 == ((line >> bit_index) & 0x1)));
                    // the value of the current pixel on the screen
                    int y = (row + byte_index) %32 ;
                    int x = ((col + (7 - bit_index))%64  );
                    bool pixelp = framebuffer[x + y * 64];

                    // if drawing to the screen would cause any pixel to be erased,
                    // set the collision flag to 1
                    if (bit && pixelp)
                    {
                         VN[0xF] = 1;
                    }

                    // draw this pixel by XOR
                   
                    framebuffer[x +y*64] =  bit^pixelp;
                    //System.Diagnostics.Debug.Write("|"+(pixelp ^ bit )+"|"+ row + "|" + byte_index + "|" + col + "|" + bit_index  +"|" + (row + byte_index) % 64 + (col + (7 - bit_index)) % 32+"|"+ (row + byte_index) % 64 + (col + (7 - bit_index)) % 32);
                    
                }
               
            }
        }
        private void clearScreen()
        {
            System.Diagnostics.Debug.WriteLine("clearScreen");
            framebuffer = new bool[64 * 32];
        }
        public void LoadRom(string path)
        {
            string pathSource = @"C:\Development\TETRIS";

            try
            {

                using (FileStream fsSource = new FileStream(pathSource,
                    FileMode.Open, FileAccess.Read))
                {

                    // Read the source file into a byte array.
                    byte[] rom = new byte[fsSource.Length];
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = fsSource.Read(rom, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    for(int i = 0; i < rom.Length; i++)
                    {
                        memory[i + 512] = rom[i];
                    }
                   
                   
                }
            }
            catch (FileNotFoundException ioEx)
            {
                Console.WriteLine(ioEx.Message);
            }
             byte[] fontset = new byte[80]
            {
                0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                0x20, 0x60, 0x20, 0x20, 0x70, // 1
                0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                0xF0, 0x80, 0xF0, 0x80, 0x80  // F 
            };
            for (int i = 0; i < fontset.Length; i++)
            {
                memory[i] = fontset[i];
            }
        }
    }
}
