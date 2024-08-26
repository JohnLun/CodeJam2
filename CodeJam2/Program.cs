using Figgle;
using SixLabors.ImageSharp;
using Spectre.Console;
using System.Security.Cryptography.X509Certificates;
using System.Text;
namespace CodeJam2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(FiggleFonts.Standard.Render("BLACKJACK"));
            Console.OutputEncoding = Encoding.UTF8;
            string keepPlaying = "Y";

            //Welcome message
            Console.WriteLine("Welcome to BlackJack!");

            //initialize Game
            Console.WriteLine("How much are you playing with today?");

            //initializing player, dealer, deck objects
            int cashStack = int.Parse(Console.ReadLine());
            Player player = new Player(cashStack);
            Dealer dealer = new Dealer();
            Deck deck = new Deck();
            Console.Clear();

            //While player has enough money and chooses to keep playing
            while (player.CanPlay() && keepPlaying == "Y")
            {
                //Printing messages
                Console.WriteLine(FiggleFonts.Standard.Render("BLACKJACK"));
                Console.WriteLine("Current Balance:");
                Console.ForegroundColor = ConsoleColor.Green;

                //Current Cash stack
                Console.WriteLine(FiggleFonts.Standard.Render($"${player.GetCashStack()}"));
                Console.ResetColor();

                //Resetting Player and Dealer hands
                player.FullReset();
                dealer.FullReset();
                List<int> playerHand = new List<int>();
                List<int> dealerHand = new List<int>();
                
                //checking for valid wager
                Console.WriteLine("How much would you like to wager?");
                int wager = int.Parse(Console.ReadLine());
                while (!player.CanWager(wager))
                {
                    Console.WriteLine("Please enter a valid amount");
                    wager = int.Parse(Console.ReadLine());
                }

                //Dealing out cards to player and dealer
                int[] hands = deck.Deal();
                player.Add(hands[0]);
                player.Add(hands[2]);

                dealer.AddScore(hands[1]);
                dealer.AddScore(hands[3]);

                //Printing Dealer + Player hands to console
                deck.PrintHand(dealer.GetHand());
                Console.WriteLine("\n");
                deck.PrintHand(player.GetHand());

                //Keep dealing while player keeps hitting
                string ans = "";
                bool playerLost = false;
                while (ans != "S")
                {
                    Console.WriteLine("Hit or Stand? [H/S]");
                    ans = Console.ReadLine().ToUpper();
                    if (ans == "H")
                    {
                        player.Add(deck.Hit());
                        deck.PrintHand(dealer.GetHand());
                        Console.WriteLine("\n");
                        deck.PrintHand(player.GetHand());
                        Console.WriteLine("\n");
                    }

                    //Player loses if greater than 21, restarts hand
                    if(player.Bust())
                    {
                        Console.WriteLine("BUSTED!");
                        playerLost = true;
                        break;
                    }
                }


                //If player lost, output how much player lost
                if(playerLost)
                {
                    Console.WriteLine("You Lost:\n");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"-${wager}");
                    Console.ResetColor();
                    Thread.Sleep(3000);
                    Console.Clear();

                    continue;
                }
                bool dealerBust = false;

                //Keep dealing to dealer while below score of 17
                while (dealer.MustHit())
                {
                    dealer.AddScore(deck.Hit());
                    deck.PrintHand(dealer.GetHand());
                    deck.PrintHand(player.GetHand());
                    Console.WriteLine("\n");

                    //If dealer busts, player wins
                    if (dealer.Bust())
                    {
                        dealerBust = true;
                        break;
                    }
                }

                //Checking if player's score greater than dealer's score

                if (player.GetTotScore() >= dealer.GetTotScore() || dealerBust)
                {
                    Console.WriteLine("You Won:\n");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"+${wager * 2}");
                    Console.ResetColor();
                    player.Win();
                } else
                {
                    Console.WriteLine("You Lost:\n");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"-${wager}");
                    Console.ResetColor();
                }

                //Printing new cash stack
                Console.WriteLine("New Balance:\n");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(FiggleFonts.Standard.Render($"${player.GetCashStack()}"));
                Console.ResetColor();
                Console.WriteLine("Play Again? [Y/N]");
                keepPlaying = Console.ReadLine().ToUpper();
                Console.Clear();
                deck = new Deck();

            }
        }
        class Deck
        {
            Dictionary<int, int> deckOfCards;
            public Deck()
            {
                deckOfCards = new Dictionary<int, int>();
                InitializeDeck();
            }
            public void InitializeDeck()
            {
                //Create a deck of cards
                for(int i = 1; i < 14; i++)
                {                   
                    deckOfCards.Add(i, 4);
                }
            }

            public int[] Deal()
            {
                //Deal 4 cards, 2 to player, 2 to dealer
                int[] hands = new int[4];
                for(int i = 0; i < 4; i++)
                {
                    hands[i] = Hit();
                }
                return hands;
            }

            public int Hit()
            {
                //Grabs a random card from the Deck and returns the value
                Random rnd = new Random();
                int randomCardVal = rnd.Next(2, 14);
                while (deckOfCards[randomCardVal] == 0)
                {
                    randomCardVal = rnd.Next(2, 14);
                }
                deckOfCards[randomCardVal] -= 1;
                return randomCardVal;
            }

            //Prints the hand
            // There shoud be more clarification on which hand is mine vs the Dealer
            public void PrintHand(List<int> hand)
            {
                StringBuilder toRender = new StringBuilder();
                foreach (int cardVal in hand)
                {
                   
                    switch (cardVal)
                    {
                        case 11:
                            toRender.Append("J ");
                            break;
                        case 12:
                            toRender.Append("Q ");
                            break;
                        case 13:
                            toRender.Append("K ");
                            break;
                        default:
                            toRender.Append(cardVal.ToString() + " ");
                            break;
                    }
                    
                }
                
                Console.Write(FiggleFonts.Standard.Render(toRender.ToString()));
            }

            //Prints the deck
            public void PrintDeck()
            {
                foreach (var card in deckOfCards)
                {
                    Console.WriteLine($"{card.Key} : {card.Value}");
                }
            }
        }

        class Dealer
        {
            private List<int> dealerHand;
            private int totScore;
            private int alternateScore;
            public Dealer()
            {
                this.totScore = 0;
                dealerHand = new List<int>();
            }
            
            //Check if Bust
            public bool Bust()
            {
                if (this.totScore > 21)
                {
                    dealerHand = new List<int>();
                    return true;
                }
                return false;
            }

            //Reset hand
            public void FullReset()
            {
                dealerHand = new List<int>();
                totScore = 0;
            }

            //Add Card value to current total score
            public void AddScore(int valOfCard)
            {
                if (valOfCard == 1)
                {
                    alternateScore += 11;
                    totScore += 1;
                } else
                {
                    totScore += Math.Min(10, valOfCard);
                    dealerHand.Add(valOfCard);
                }
            }

            //Check if dealer has to hit
            public bool MustHit()
            {
                return totScore < 17;
            }

            //Get dealer's current score
            public int GetTotScore()
            {
                return this.totScore;
            }

            //Get dealer's current hand
            public List<int> GetHand()
            {
                return dealerHand;
            }
        }

        class Player
        {
            private List<int> playerHand;
            private int cashStack;
            private int currWager;
            private int totScore;
            public Player(int cashStack)
            {
                this.playerHand = new List<int>();
                this.cashStack = cashStack;
                this.currWager = 0;
                this.totScore = 0;
            }
            //Check if Player has enough money
            public bool CanPlay()
            {
                return cashStack > 0;
            }

            //Check if Player can wager 
            public bool CanWager(int wager)
            {
                if (wager > cashStack)
                {
                    return false;
                }
                currWager = wager;
                this.cashStack -= wager;
                return true;
            }

            //Get player's current cash stack
            public int GetCashStack()
            {
                return this.cashStack;
            }

            //Reset player hand
            public void FullReset()
            {
                playerHand = new List<int>();
                totScore = 0;
            }

            //Get player's current hand value
            public int GetTotScore()
            {
                return this.totScore;
            }
            
            //Add winning wager to player's cash stack
            public void Win()
            {
                this.cashStack += currWager * 2;
            }

            //Add value of card to player's total score
            public void Add(int valOfCard)
            {
                this.totScore += Math.Min(10, valOfCard);
                playerHand.Add(valOfCard);
            }

            //Check if player busts
            public bool Bust()
            {
                if(this.totScore > 21)
                {
                    totScore = 0;
                    playerHand = new List<int>();
                    return true;
                }
                return false;
            }

            public List<int> GetHand()
            {
                return playerHand;
            }
        }

        class Card
        {
            private int val;
            public Card(int val)
            {
                this.val = val;
            }
            public int GetVal()
            {
                return val;
            }
            public void SetVal(int val)
            {
                this.val = val;
            }


        }

    }
}
