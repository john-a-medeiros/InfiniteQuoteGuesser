using System.Net;
using InfiniteQuoteGuesser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfiniteQuoteGuesser.Pages;

public class IndexModel : PageModel
{
    
    private readonly ILogger<IndexModel> _logger;

    public QuoteDetailed[] currRoundQuotes = new QuoteDetailed[4];
    public int currWinner;

    string selectedOption { get; set; }
    string displayQuote { get; set; } = "";

    int currentStreak = 0;
    int bestStreak = 0;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public string getDisplay()
    {
        return displayQuote;
    }

    public string getAuthorZero()
    {
        if (currRoundQuotes[0] != null)
        {
            return currRoundQuotes[0].author;
        }

        return "";
    }

    public string getAuthorOne()
    {
        if (currRoundQuotes[1] != null)
        {
            return currRoundQuotes[1].author;
        }

        return "";
    }

    public string getAuthorTwo()
    {
        if (currRoundQuotes[2] != null)
        {
            return currRoundQuotes[2].author;
        }

        return "";
    }

    public string getAuthorThree()
    {
        if (currRoundQuotes[3] != null)
        {
            return currRoundQuotes[3].author;
        }

        return ""; ;
    }

    public string getCurrentStreak()
    {
        //Retrieving streak from TempData and then readding the info
        currentStreak = (int)TempData["currentStreak"];
        TempData["currentStreak"] = currentStreak;
        return currentStreak.ToString();
    }

    public string getBestStreak()
    {
        //Retrieving streak from TempData and then readding the info
        bestStreak = (int)TempData["bestStreak"];
        TempData["bestStreak"] = bestStreak;
        return bestStreak.ToString();
    }

    public string getRightWrong()
    {
        var rightOrWrong = TempData["rightOrWrong"];
        if (rightOrWrong == null)
        {
            return "";
        }
        else
            return (string)rightOrWrong;
    }

    public string getPrevAnswer()
    {
        var prevA = TempData["prevA"];
        if (prevA == null)
        {
            return "";
        }
        else
            return (string)prevA;
    }

    public async Task<PageResult> OnGet()
    {
        //Initializing TempData for currentStreak, bestStreak
        TempData["currentStreak"] = 0;
        TempData["bestStreak"] = 0;

        //Starting a round, returning page
        await StartRound();
        return new PageResult();
    }

    public async Task<int> StartRound()
    {
        await RefreshQuotes();

        //Randomly selecting which quote to display and consider winner
        Random rnd = new Random();
        currWinner = (rnd.Next()) % 4;
        displayQuote = currRoundQuotes[currWinner].quote;

        TempData["currA"] = currRoundQuotes[currWinner].author;

        //Using TempData in order to save currWinner across html requests
        TempData["winningGuess"] = currWinner;

        return 1;
    }

    public async Task<PageResult> OnPostAsync()
    {
        //Retrieivng the answer selected by the user before checking if its right or wrong
        var sel = Request.Form["selectOptions"];
        selectedOption = sel.ToString();
        int selected;

        //Getting currWinner, currStreak, bestStreak from TempDate
        currWinner = (int)TempData["winningGuess"];
        currentStreak = (int)TempData["currentStreak"];
        bestStreak = (int)TempData["bestStreak"];



        //Checking to see if the correct quote was selected
        switch (selectedOption)
        {
            case "Guess0":
                selected = 0;
                break;
            case "Guess1":
                selected = 1;
                break;
            case "Guess2":
                selected = 2;
                break;
            case "Guess3":
                selected = 3;
                break;
            default:
                selected = 0;
                break;
        }

        //Updating streaks if chosen correctly
        if (selected == currWinner)
        {
            currentStreak += 1;
            TempData["currentStreak"] = currentStreak;
            if (currentStreak > bestStreak)
            {
                bestStreak = currentStreak;
            }

            //Updating right or wrong
            TempData["rightOrWrong"] = "Correct!";
        }

        //Reseting current streak if chosen incorrectly
        else
        {
            currentStreak = 0;
            TempData["rightOrWrong"] = "Incorrect!";
        }
        TempData["currentStreak"] = currentStreak;
        TempData["bestStreak"] = bestStreak;
        TempData["prevA"] = (string)TempData["currA"];

        //Starting new round and returning page
        await StartRound();
        return new PageResult();
    }

    public async Task<int> RefreshQuotes()
    {
        //Method which retrieves relevant quotes and randomly picks winner
        string JsonResponse = "";
        string aK = Models.WebKeyGetter.getK();

        //Retrieving quote from API ninja
        //NOTE: Add update later for proper error handling
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.api-ninjas.com/v1/quotes?limit=5");
        request.Method = "GET";
        request.ContentType = "application/JSON";
        request.Headers.Add("X-Api-Key", aK);

        HttpWebResponse webResponse = (HttpWebResponse)await request.GetResponseAsync();

        using (Stream responseStream = webResponse.GetResponseStream())
        {
            using (StreamReader reader = new StreamReader(responseStream))
            {
                JsonResponse = reader.ReadToEnd();
            }
        }
        webResponse.Close();

        //Instantiating quotes array
        for (int i = 0; i < 4; i++)
        {
            currRoundQuotes[i] = new QuoteDetailed();
        }

        //Indecies to keep track of quote, author, and category locations within the text
        int quoteDex = JsonResponse.IndexOf("\"quote\"");
        int authorDex = JsonResponse.IndexOf("\"author\"");
        int categoryDex = JsonResponse.IndexOf("\"category\"");

        //Getting each quote, doing some funky math to get the length 
        for (int i = 0; i < 4; i++)
        {
            currRoundQuotes[i].quote = JsonResponse.Substring(quoteDex + 10, authorDex - quoteDex - 10 - 3);

            currRoundQuotes[i].author = JsonResponse.Substring(authorDex + 11, categoryDex - authorDex - 11 - 3);

            //Finding next quoteIndex for use in category and for use in next loop
            quoteDex = JsonResponse.IndexOf("\"quote\"", quoteDex + 1);

            currRoundQuotes[i].category = JsonResponse.Substring(categoryDex + 13, quoteDex - categoryDex - 12 - 6);

            //Finding authorIndex and categoryIndex for use in next loop
            authorDex = JsonResponse.IndexOf("\"author\"", authorDex + 1);
            categoryDex = JsonResponse.IndexOf("\"category\"", categoryDex + 1);

        }

        return 0;
    }
}

