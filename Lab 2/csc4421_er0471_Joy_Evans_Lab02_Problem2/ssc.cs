
            ScrapeSureCritic.ScrapeDealership();
        }
    }
    /// <summary>
    /// Review data.
    /// </summary>
    public class ReviewData
    {
        public string dealership { get; set; }
        public string dealerID { get; set; }
        public string reviewer { get; set; }
        public string reviewText { get; set; }
        public float numOfStars { get; set; }
        public string dateOfReview { get; set; }
        public string dateOfScrape { get; set; }
        public string source = "SureCritic";

    }
    /// <summary>
    /// Db connect.
    /// </summary>
    public class DbConnect
    {
        public static SqlConnection GetConnection()
        {


            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {

                DataSource = "localhost",
                UserID = "sa",
                Password = "O2F0dmGdbBFG",
                InitialCatalog = "RA"
            };

            return new SqlConnection(builder.ConnectionString);
        }

    }
    /// <summary>
    /// Scrape sure critic.
    /// </summary>
    public static class ScrapeSureCritic
    {
        public static void ScrapeDealership()
        {
            //---------------------------VARIABLES-----------------------------//
            //string for type of review (IRF or OEM) in SQL-Review Table
            string OEM = "OEM";
            //2 for SureCritic
            int source = 2;

            //-----------------------------LISTS-------------------------------//

            //holds list of dealer names for first portion of search string for scrapping
            List<string> dealershipName = new List<string>();
            //list of dealership IDs used for SQL statements for adding/updating UniqueID in Reviews table
            List<string> dealershipID = new List<string>();
            //list of zip code used for scrapping
            List<string> dealershipZipCode = new List<string>();
            //populates dealershipName list
            GetDealershipName(dealershipName);
            //populates dealershipID list
            GetDealershipID(dealershipID);
            //populates dealershipZipCode list
            GetDealershipZipCode(dealershipZipCode);
            //creates new ReviewData type to store scraped data
            List<ReviewData> reviewData = new List<ReviewData>();
            ReviewData review = new ReviewData();

            //begins scraping based on dealershipName Count
            for (int i = 0; i < dealershipName.Count; i++)
            {
                using (var driver = CreateDriver())
                {
                    string Name = dealershipName[i];
                    string ZipCode = dealershipZipCode[i];
                    string DealerID = dealershipID[i];

                    //Press search button using dealer name and zipcode for search
                    SureCriticSearchPress(Name, ZipCode, driver);

                    //allow page to load
                    Wait(driver);
                    int reviewCount = 0;

                    //retrieves number of reviews
                    reviewCount = ParseReviewCount(reviewCount, driver);

                    //if there are no reviews, search for next dealer
                    if (reviewCount == 0)
                    {

                        i++;
                        continue;
                    }
                    //if there are reviews, click on review count link
                    else
                    {


                        driver.FindElement(By.CssSelector(".reviews.pull-right")).Click();
                        //wait for page to load
                        Wait(driver);
                        int lastPage = 0;
                        int NumOfReviews = 0;
                        int newReviews = 0;
                        int counter = 1;
                        double calc = 0.0;
                        string page = "?page=";

                        HtmlWeb web = new HtmlWeb();
                        HtmlDocument site = web.Load(driver.Url);



                        var NumReviews = site.DocumentNode.SelectNodes("//h3[@class='review-count']");

                        //find last page
                        foreach (var number in NumReviews)
                        {
                            var NumString = Regex.Match(number.InnerText, @"\d+").Value;
                            NumOfReviews = Int32.Parse(NumString);
                            calc = NumOfReviews / 20.00;
                            lastPage = (int)Math.Ceiling(calc);
                            Console.WriteLine(lastPage);

                        }
                        //retrieve number of SureCritic reviews for dealer in database
                        int totalReviewCount = GetSCReviewCount(DealerID);
                        Console.WriteLine("Database Review Count: " + totalReviewCount);
                        Console.WriteLine("SC Review Count: " + NumOfReviews);

                        //if there are no new reviews, search for next dealer
                        if (totalReviewCount == NumOfReviews)
                        {

                            i++;
                            continue;

                        }
                        //if there are new reviews, only scrape pages with new reviews (Sort by Most Recent)
                        if (NumOfReviews > totalReviewCount)
                        {

                            newReviews = NumOfReviews - totalReviewCount;
                            calc = newReviews / 20.00;
                            lastPage = (int)Math.Ceiling(calc);
                            InsertSCReviewCount(NumOfReviews, DealerID);


                        }
                        else
                        {
                            InsertSCReviewCount(NumOfReviews, DealerID);
                        }

                        //while current page is less than last page
                        while (counter <= lastPage)
                        {
                            //load current page based on counter increment
                            HtmlDocument dealerSite = web.Load(driver.Url + page + counter);

                            foreach (HtmlNode node in dealerSite.DocumentNode.SelectNodes("//div[@class='review-body']"))
                            {
                                //store review date in review list
                                var dateNode = node.SelectSingleNode(".//span[@class='review-date']");
                                review.dateOfReview = dateNode.InnerText;

                                //store number of stars in review list
                                var stars = node.SelectSingleNode(".//img[@class='stars-medium']").GetAttributeValue("alt", "");
                                var numStars = Regex.Match(stars, @"\d+").Value;
                                review.numOfStars = float.Parse(numStars);

                                //store review text in review list
                                var reviews = "";
                                var reviewContent = node.SelectSingleNode(".//div[@class='review-text top-spacer-sm']");

                                if (reviewContent == null)
                                {
                                    reviews = "";

                                }
                                else
                                {
                                    reviews = reviewContent.InnerText;
                                    review.reviewText = reviews;

                                }
                                review.dateOfScrape = DateTime.Now.ToString();
                                reviewData.Add(review);

                                //print current page
                                Console.Write(counter + "/" + lastPage + "\n");
                                //sql query to insert into database
                                string reviewAdd = "INSERT into ReviewTest (DealerName,DealerID,ReviewStars,ReviewContent,ReviewDate,ScrapeDate,ReviewSource) SELECT @name,@id,@stars,@content,@reviewdate,@scrapedate,@source WHERE NOT EXISTS" +
        "(SELECT * FROM ReviewTest WHERE Dealername = @name AND ReviewContent=@content AND ReviewDate = @reviewdate);";
                                using (SqlConnection connection = DbConnect.GetConnection())
                                {
                                    connection.Open();

                                    using (SqlCommand cmd = new SqlCommand(reviewAdd, connection))
                                    {

                                        //write to database and print to console
                                        for (int j = 0; j < reviewData.Count; j++)
                                        {
                                            Console.WriteLine(dealershipName[i]);
                                            Console.WriteLine(dealershipID[i]);
                                            Console.WriteLine(reviewData[j].dateOfScrape);
                                            Console.WriteLine(reviewData[j].dateOfReview);
                                            Console.WriteLine(reviewData[j].numOfStars);
                                            Console.WriteLine(reviewData[j].reviewText);
                                            cmd.Parameters.AddWithValue("@name", dealershipName[i]);
                                            cmd.Parameters.AddWithValue("@id", dealershipID[i]);
                                            cmd.Parameters.AddWithValue("@stars", reviewData[j].numOfStars);
                                            cmd.Parameters.AddWithValue("@content", reviewData[j].reviewText);
                                            cmd.Parameters.AddWithValue("@reviewdate", reviewData[j].dateOfReview);
                                            cmd.Parameters.AddWithValue("@scrapedate", reviewData[j].dateOfScrape);
                                            cmd.Parameters.AddWithValue("@source", "SureCritic");


                                            cmd.ExecuteNonQuery();

                                        }

                                    }

                                }

                                reviewData.Clear();

                            }
                            Console.WriteLine("New Reviews Added: " + newReviews);
                            counter++;

                        }


                    }



                }

            }
            dealershipName.Clear();
            dealershipID.Clear();
            dealershipZipCode.Clear();
        }

        //function to input dealership name and zip code into search and press search button
        public static void SureCriticSearchPress(string searchName, string searchZip, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl("https://www.surecritic.com");
            IWebElement nameSubmitElement = driver.FindElement(By.Id("term"));
            IWebElement zipSubmitElement = driver.FindElement(By.Id("near"));
            Wait(driver);
            nameSubmitElement.Click();
            nameSubmitElement.SendKeys(searchName);
            zipSubmitElement.Click();
            zipSubmitElement.Clear();
            zipSubmitElement.SendKeys(searchZip);
            IWebElement searchButton = driver.FindElement(By.XPath("//*[@id=\"homepage-search\"]/button/i"));
            searchButton.Click();



        }

        //retrieves review count
        public static int ParseReviewCount(int reviewCount, ChromeDriver driver)
        {
            Wait(driver);
            if (ElementExistsTest(By.CssSelector(".reviews.pull-right"), driver))
            {


                var reviewCountElement = driver.FindElement(By.CssSelector(".reviews.pull-right")).Text;
                Console.WriteLine(reviewCountElement);
                string[] token = reviewCountElement.Split(' ');
                reviewCount = Int32.Parse(token[0]);
            }
            else
            {

                reviewCount = 0;
            }

            return reviewCount;


        }

        //checks if html element exists on page
        private static bool ElementExistsTest(By by, ChromeDriver driver)
        {

            try
            {

                driver.FindElement(by);
                return true;

            }
            catch (NoSuchElementException)
            {
                return false;
            }


        }
        //--------------------------------------CALLABLE FUNCTIONS------------------------------------------//

        //-------------------------Dealership ID, Name, and address get functions---------------------------//

        private static List<string> GetDealershipName(List<string> dealershipName)
        {
            //Sample data pull from database & assign dealerships to list
            using (SqlConnection connection = DbConnect.GetConnection())
            {
                string sql = "SELECT DSC from HyundaiDealershipData WHERE Dealer LIKE 'MI%'";
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();
                SqlDataReader r = command.ExecuteReader();
                if (r.HasRows)
                {
                    int i = 0;
                    while (r.Read())
                    {
                        var dealerString = r.GetString(0);
                        dealershipName.Insert(i, dealerString);
                    }
                }
                r.Close();
                connection.Close();
            }

            return dealershipName;
        }

        private static List<string> GetDealershipID(List<string> dealershipID)
        {
            //Sample data pull from database & assign dealerships to list
            using (SqlConnection connection = DbConnect.GetConnection())
            {
                string sql = "SELECT Dealer from HyundaiDealershipData WHERE Dealer LIKE 'MI%'";
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();
                SqlDataReader r = command.ExecuteReader();
                if (r.HasRows)
                {
                    int i = 0;
                    while (r.Read())
                    {
                        var dealerString = r.GetString(0);
                        dealershipID.Insert(i, dealerString);
                    }
                }
                r.Close();
                connection.Close();
            }

            return dealershipID;
        }
        private static List<string> GetDealershipZipCode(List<string> dealershipZipCode)
        {
            //Sample data pull from database & assign dealerships to list
            using (SqlConnection connection = DbConnect.GetConnection())
            {
                string sql = "SELECT Address from HyundaiDealershipData WHERE Dealer LIKE 'MI%'";
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();
                SqlDataReader r = command.ExecuteReader();
                if (r.HasRows)
                {
                    int i = 0;
                    while (r.Read())
                    {
                        var dealerString = r.GetString(0);
                        string dealerZip = dealerString.Substring(dealerString.Length - 5);
                        dealershipZipCode.Insert(i, dealerZip);

                    }
                }
                r.Close();
                connection.Close();
            }

            return dealershipZipCode;
        }
        private static void InsertSCReviewCount(int reviewCount, string dealerID)
        {

            using (SqlConnection connection = DbConnect.GetConnection())
            {
                string sql = "UPDATE HyundaiDealershipData SET SureCriticReviewCount='" + reviewCount + "'" + "WHERE Dealer='" + dealerID + "'";
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();
                command.ExecuteNonQuery();

            }
        }

        //retireves number of SureCritic reviews from database
        private static int GetSCReviewCount(string dealerID)
        {
            using (SqlConnection connection = DbConnect.GetConnection())
            {
                string sql = "SELECT COUNT (DealerID) FROM ReviewTest WHERE DealerID='" + dealerID + "' AND ReviewSource = 'SureCritic'";
                int oldReviewCount = 0;
                SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();
                SqlDataReader r = command.ExecuteReader();
                if (r.HasRows)
                {

                    while (r.Read())
                    {
                        if (r.IsDBNull(0))
                        {
                            continue;
                        }
                        else
                        {
                            oldReviewCount = r.GetInt32(0);
                        }


                    }
                }
                r.Close();
                return oldReviewCount;

            }

        }

        //creates ChromeDriver
        private static ChromeDriver CreateDriver()
        {

            var options = new ChromeOptions();

            return new ChromeDriver(options);

        }

        //Wait function used for allowing pages to load
        private static void Wait(ChromeDriver driver)
        {

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);



        }
    }
}
