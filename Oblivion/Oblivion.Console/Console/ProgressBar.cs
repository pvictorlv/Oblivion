/*
 * Created by SharpDevelop.
 * User: claudio.santoro
 * Date: 02/10/2014
 * Time: 16:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mercury
{

public abstract class AbstractBar
    {
        public AbstractBar()
        {

        }

        /// <summary>
        /// Prints a simple message 
        /// </summary>
        /// <param name="msg">Message to print</param>
        public void PrintMessage(string msg)
        {
            Console.Write("  " + msg);
            Console.Write( "\r".PadLeft( Console.WindowWidth - Console.CursorLeft - 1 ) );
        }

        public abstract void Step();
    }
    
    
public class AnimatedBar : AbstractBar
    {
        List<string> animation;
        int counter;
        public AnimatedBar() : base()
        {
            this.animation = new List<string> { "/", "-", @"\", "|" };
            this.counter = 0;
        }

        /// <summary>
        /// prints the character found in the animation according to the current index
        /// </summary>
        public override void Step() {
            Console.Write(this.animation[this.counter]+"\b");
            this.counter++;
            if (this.counter == this.animation.Count)
                this.counter = 0;
        }
    }
}
