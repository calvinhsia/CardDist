﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

/*
 File->new->Project->C#->WPF App "CardDist"
 download cards.dll from https://onedrive.live.com/redir?resid=D69F3552CEFC21!74629&authkey=!AGaX84aRcmB1fB4&ithint=file%2cDll
 Solution->Add Existing Item Cards.dll (Properties: Copy to Output Directory=Copy If Newer)
 Add Project->Add Reference to System.Drawing
     * 
     * * */
namespace CardDist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int _hghtCard = 150;
        public static int _wdthCard = 130;
        public MainWindow()
        {
            InitializeComponent();
            Width = 1200;
            Height = 800;
            Title = "CardDist";
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var sp = new StackPanel() { Orientation = Orientation.Vertical };
                sp.Children.Add(new Label() { Content = "Card Dealing Program. Click to toggle dealing" });
                var canvas = new Canvas();
                sp.Children.Add(canvas);
                this.Content = sp;
                //for (var suit = 0; suit < 4; suit++)
                //{
                //    for (var denom = 0; denom < 13; denom++)
                //    {
                //        // create a new image for a card
                //        var img = new Image()
                //        {
                //            Source = Cards.GetCard((Cards.Suit)suit, denom),
                //            Height = hghtCard
                //        };
                //        // add it to the canvas
                //        canvas.Children.Add(img);
                //        // set it's position on the canvas
                //        Canvas.SetLeft(img, denom * wdthCard);
                //        Canvas.SetTop(img, suit * hghtCard);
                //    }
                //}
                //for (int i = 0; i < Cards.NumCardBacks; i++)
                //{
                //    var img = new Image()
                //    {
                //        Source = Cards.GetCardBack(i),
                //        Height = hghtCard
                //    };
                //    canvas.Children.Add(img);
                //    Canvas.SetTop(img, hghtCard * 5);
                //    Canvas.SetLeft(img, i * wdthCard);
                //}
                var rand = new Random(1);
                int[] deck = new int[52];
                for (var suit = 0; suit < 4; suit++)
                {
                    for (var denom = 0; denom < 13; denom++)
                    {
                        var num = suit * 13 + denom;
                        deck[num] = num; 
                        // create a new image for a card
                        var img = new Image()
                        {
                            Source = Cards.GetCard((Cards.Suit)suit, denom),
                            Height = _hghtCard
                        };
                        // add it to the canvas
                        canvas.Children.Add(img);
                        this.SaveImageToFile(img, $"Card_{suit}_{denom}.jpg");
                        // set it's position on the canvas
                        Canvas.SetLeft(img, denom * _wdthCard);
                        Canvas.SetTop(img, (int)suit * _hghtCard);
                    }
                }
                for (int i = 0; i < Cards.NumCardBacks; i++)
                {
                    var img = new Image()
                    {
                        Source = Cards.GetCardBack(i),
                        Height = _hghtCard
                    };
                    canvas.Children.Add(img);
                    Canvas.SetTop(img, _hghtCard * 5);
                    Canvas.SetLeft(img, i * _wdthCard);
                }

                var timer = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(400),
                    DispatcherPriority.Normal,
                    (o, args) =>
                    {
                        canvas.Children.Clear();
                        // shuffle
                        for (int n = 0; n < 52; n++)
                        {
                            var tempNdx = rand.Next(52);
                            var tempSrc = deck[tempNdx];
                            deck[tempNdx] = deck[n];
                            deck[n] = tempSrc;
                        }
                        // deal
                        var north = new Hand(deck.Skip(0).Take(13).ToList());
                        var south = new Hand(deck.Skip(13).Take(13).ToList());
                        var east = new Hand(deck.Skip(26).Take(13).ToList());
                        var west = new Hand(deck.Skip(39).Take(13).ToList());
                        canvas.Children.Add(north);
                        Canvas.SetTop(north, 0);
                        Canvas.SetLeft(north, 300);
                        canvas.Children.Add(south);
                        Canvas.SetTop(south, 500);
                        Canvas.SetLeft(south, 300);
                        canvas.Children.Add(west);
                        Canvas.SetTop(west, 200);
                        Canvas.SetLeft(west, 0);
                        canvas.Children.Add(east);
                        Canvas.SetTop(east, 200);
                        Canvas.SetLeft(east, 700);

                    },
                    this.Dispatcher);
                this.MouseUp += (om, em) =>
                {
                    timer.IsEnabled = !timer.IsEnabled;
                };
            }
            catch (Exception ex)
            {
                this.Content = ex.ToString();
            }
        }

        private void SaveImageToFile(Image img, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                var jpgEncoder = new JpegBitmapEncoder();
                var bmpframe = BitmapFrame.Create((BitmapSource)img.Source);
                jpgEncoder.Frames.Add(bmpframe);
                jpgEncoder.Save(fs);
            }

        }

        /// <summary>
        /// holds 13 cards for e.g. N,S,E,W 
        /// </summary>
        public class Hand : Canvas, IComparer
        {
            Card[] _cards;
            public Hand(List<int> list)
            {
                _cards = new Card[13];
                var l = new List<Card>();
                for (var i = 0; i < 13; i++)
                {
                    var card = new Card(list[i]);
                    l.Add(card);
                    _cards[i] = card;
                }
                // sorting of cards in a hand is not the same
                // order as C,D,H,S: we want to alternate red/black
                Array.Sort(_cards, this);

                for (var i = 0; i < 13; i++)
                {
                    this.Children.Add(_cards[i]);
                    Canvas.SetLeft(_cards[i], i * MainWindow._wdthCard / 7);
                }
                var label = new Label()
                {
                    Content = GetPoints.ToString()
                };
                this.Children.Add(label);
                Canvas.SetTop(label, MainWindow._hghtCard + 5);
            }

            public int GetPoints
            {
                get
                {
                    return _cards.Sum(c => c.Points);
                }

            }
            public int Compare(object x, object y)
            {
                if (x as Card != null && y as Card != null)
                {
                    var xCard = x as Card;
                    var yCard = y as Card;
                    if (xCard._suit == yCard._suit)
                    {
                        return yCard._denom.CompareTo(xCard._denom);
                    }
                    switch (xCard._suit)
                    {
                        case Cards.Suit.Spades:
                            return -1;
                        case Cards.Suit.Hearts:
                            if (yCard._suit == Cards.Suit.Spades)
                            {
                                return 1;
                            }
                            return -1;
                        case Cards.Suit.Clubs:
                            if (yCard._suit == Cards.Suit.Spades || yCard._suit == Cards.Suit.Hearts)
                            {
                                return 1;
                            }
                            return -1;
                        case Cards.Suit.Diamonds:
                            return 1;
                    }

                }
                throw new InvalidOperationException();
            }
        }

        public class Card : Image, IComparable
        {
            public Cards.Suit _suit;
            public int _denom;
            public Card(int value)
            {
                int suit = value / 13;
                _suit = (Cards.Suit)suit;
                _denom = value - suit * 13;
                Source = Cards.GetCard(_suit, _denom);
                Height = MainWindow._hghtCard;
            }

            // A=12, K=11, Q=10, J = 9. Pts = denom - 9
            public int Points { get { return _denom >= 9 ? _denom - 8 : 0; } }

            public int CompareTo(object obj)
            {
                if (obj as Card != null)
                {
                    var other = (Card)obj;
                    if (_suit == other._suit)
                    {
                        return _denom.CompareTo(other._denom);
                    }
                    return _suit.CompareTo(other._suit);
                }
                throw new InvalidOperationException();
            }
            public override string ToString()
            {
                return $"{_denom} of {_suit}";
            }
        }

        public class Cards
        {
            public enum Suit
            {
                Clubs = 0,
                Diamonds = 1,
                Hearts = 2,
                Spades = 3
            }
            // put cards in 2 d array, suit, rank (0-12 => 2-A)
            private BitmapSource[,] _bitmapCards;
            public BitmapSource[] _bitmapCardBacks;
            private static Cards _instance;

            public static int NumCardBacks => _instance._bitmapCardBacks.Length;

            public Cards()
            {
                _bitmapCards = new BitmapSource[4, 13];
                var hmodCards = LoadLibraryEx("cards.dll", IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                if (hmodCards == IntPtr.Zero)
                {
                    throw new FileNotFoundException("Couldn't find cards.dll");
                }
                // the cards are resources from 1 - 52.
                // here is a func to load an int rsrc and return it as a BitmapSource
                Func<int, BitmapSource> GetBmpSrc = (rsrc) =>
                {
                    // we first load the bitmap as a native resource, and get a ptr to it
                    var bmRsrc = LoadBitmap(hmodCards, rsrc);
                    // now we create a System.Drawing.Bitmap from the native bitmap
                    var bmp = System.Drawing.Bitmap.FromHbitmap(bmRsrc);
                    // we can now delete the LoadBitmap
                    DeleteObject(bmRsrc);
                    // now we get a handle to a GDI System.Drawing.Bitmap
                    var hbmp = bmp.GetHbitmap();
                    // we can create a WPF Bitmap source now
                    var bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(
                        hbmp,
                        palette: IntPtr.Zero,
                        sourceRect: Int32Rect.Empty,
                        sizeOptions: BitmapSizeOptions.FromEmptyOptions());

                    // we're done with the GDI bmp
                    DeleteObject(hbmp);
                    return bmpSrc;
                };
                // now we call our function for the cards and the backs
                for (Suit suit = Suit.Clubs; suit <= Suit.Spades; suit++)
                {
                    for (int denom = 0; denom < 13; denom++)
                    {
                        // 0 -12 => 2,3,...j,q,k,a
                        int ndx = 1 + 13 * (int)suit + (denom == 12 ? 0 : denom + 1);
                        _bitmapCards[(int)suit, denom] = GetBmpSrc(ndx);
                    }
                }
                //The card backs are from 53 - 65
                _bitmapCardBacks = new BitmapSource[65 - 53 + 1];
                for (int i = 53; i <= 65; i++)
                {
                    _bitmapCardBacks[i - 53] = GetBmpSrc(i);
                }
            }

            /// <summary>
            /// Return a BitmapSource
            /// </summary>
            /// <param name="nSuit"></param>
            /// <param name="nDenom">0-12 = 2,3,4,J,Q,K,A</param>
            /// <returns></returns>
            public static BitmapSource GetCard(Suit nSuit, int nDenom)
            {
                if (_instance == null)
                {
                    _instance = new Cards();
                }
                if (nDenom < 0 || nDenom > 12)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return _instance._bitmapCards[(int)nSuit, nDenom];
            }

            internal static ImageSource GetCardBack(int i)
            {
                return _instance._bitmapCardBacks[i];
            }
        }

        public const int LOAD_LIBRARY_AS_DATAFILE = 2;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFileReserved, uint dwFlags);

        [DllImport("User32.dll")]
        public static extern IntPtr LoadBitmap(IntPtr hInstance, int uID);

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
    }
}
