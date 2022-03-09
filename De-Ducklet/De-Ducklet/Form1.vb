'' Programmer Name: Emma Gillespie
'' Date:            April 7th, 2020
'' Company Name:    Ivory Coding
'' Description:     This is an app to help with debugging based around the idea of deducking and stack overflow.

Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports MySql.Data.MySqlClient

Public Class Form1

    Dim tips(17) As String
    Dim numberOfTips As Integer
    Dim randomeNumber As Integer
    Dim Url As String = "https://www.google.com/search?q="
    Dim numResultToGet As Integer
    Private usernameArray(10000) As String
    Private titleArray(10000) As String
    Private picturePath As String
    Private arrImage() As Byte
    Private fileText As String

    '' Database stuff
    Private myConn As New MySqlConnection
    Private myCmd As New MySqlCommand
    Private myReader As MySqlDataReader
    Private results As String
    Private numberPosts As Integer = 0
    Private numberAnswers As Integer = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '' Sets database connection string
        myConn.ConnectionString = "server=192.168.1.65;user id=pi;password=69mNQ^7z*8;database=DeDucklet;persistsecurityinfo=True;port=3306;Integrated Security=False;allowpublickeyretrieval=True"

        '' Sets up the main form on load
        lblPageTitle.Text = " De-Deucking"
        pnlDeDuck.Visible() = True
        pnlSearch.Visible() = False
        pnlPostAnswer.Visible() = False
        pnlViewPost.Visible() = False
        pnlProfile.Visible() = False
        pnlSettings.Visible() = False
        LoopThroughTips()
        lblScrollTip.Text = tips(1)
        pnlSignupForm.Visible() = False
        pnlLoginPage.Visible() = True

        '' Set buttons main color
        btnSearch.BackColor = Color.FromArgb(138, 138, 138)
        btnPost.BackColor = Color.FromArgb(138, 138, 138)
        btnDeDuck.BackColor = Color.FromArgb(192, 192, 0)
        btnProfile.BackColor = Color.FromArgb(138, 138, 138)
        btnSettings.BackColor = Color.FromArgb(138, 138, 138)
    End Sub

    Public Sub LoopThroughTips()
        '' Gets the tips file and turns it into an array. Then sets label for the tips

        Dim fileName As String = "resources/tips.txt"
        Dim line As String = ""

        If (File.Exists(fileName)) Then
            Dim objReader As New StreamReader(fileName)
            Dim i As Integer = 0

            Do While (objReader.Peek() <> -1)
                i = i + 1
                line = objReader.ReadLine()
                tips(i) = line
            Loop
            numberOfTips = i
        Else
            MessageBox.Show("File does not exist. Contact Support with error code 800967-MF")
        End If
    End Sub

    Private Sub tmrNewTip_Tick(sender As Object, e As EventArgs) Handles tmrNewTip.Tick
        '' Timer for the tips to scroll
        randomeNumber = CInt(Int((numberOfTips * Rnd()) + 1))
        lblScrollTip.Text = tips(randomeNumber)
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        '' Hides all pages except the search page
        btnSearch.BackColor = Color.FromArgb(136, 46, 209)
        btnDeDuck.BackColor = Color.FromArgb(138, 138, 138)
        btnPost.BackColor = Color.FromArgb(138, 138, 138)
        btnProfile.BackColor = Color.FromArgb(138, 138, 138)
        btnSettings.BackColor = Color.FromArgb(138, 138, 138)

        pnlSearch.Visible() = True
        pnlDeDuck.Visible() = False
        pnlPostAnswer.Visible() = False
        pnlViewPost.Visible() = False
        pnlProfile.Visible() = False
        pnlSettings.Visible() = False

        lblPageTitle.Text = "Advanced Search"

        txtSearchBar.Select()
        ClearPosts()
        ClearAnswers()
    End Sub

    Private Sub btnDeDuck_Click(sender As Object, e As EventArgs) Handles btnDeDuck.Click
        '' Hides all pages except for the de ducking page
        btnDeDuck.BackColor = Color.FromArgb(192, 192, 0)
        btnSearch.BackColor = Color.FromArgb(138, 138, 138)
        btnPost.BackColor = Color.FromArgb(138, 138, 138)
        btnProfile.BackColor = Color.FromArgb(138, 138, 138)
        btnSettings.BackColor = Color.FromArgb(138, 138, 138)

        pnlSearch.Visible() = False
        pnlDeDuck.Visible() = True
        pnlPostAnswer.Visible() = False
        pnlViewPost.Visible() = False
        pnlProfile.Visible() = False
        pnlSettings.Visible() = False

        lblPageTitle.Text = " De-Deucking"
        ''txtTestScrape.Text = ""
        ClearPosts()
        ClearAnswers()
    End Sub

    Private Sub lblSearchIcon_Click(sender As Object, e As EventArgs) Handles lblSearchIcon.Click
        '' Scrapes google for the search
        '' Remove all previous labels if any
        For i As Integer = 1 To numResultToGet Step 1
            pnlResults.Controls.RemoveByKey("lblLinks" + i.ToString())
        Next
        Dim strSearch As String = txtSearchBar.Text
        strSearch = Regex.Replace(strSearch, " ", "+")
        Url = Url + strSearch + "&oq=" + strSearch '' search&oq=search
        Scrapper()
        Url = "https://www.google.com/search?q="
    End Sub

    Public Sub Scrapper()
        Dim strUrl As String = Url
        Dim strOutput As String = ""
        Dim linksArray(numResultToGet) As String

        Dim wrResponse As WebResponse
        Dim wrRequest As WebRequest = HttpWebRequest.Create(strUrl)

        ''txtTestScrape.Text = "Extracting..." & Environment.NewLine

        wrResponse = wrRequest.GetResponse()

        Using sr As New StreamReader(wrResponse.GetResponseStream())
            strOutput = sr.ReadToEnd()
            sr.Close()
        End Using

        strOutput = Regex.Replace(strOutput, "<!(.|\s)*?>", "")
        ''strOutput = Regex.Replace(strOutput, "</?[a-z][a-z0-9]*[^<>]*>", "")
        ''strOutput = Regex.Replace(strOutput, "<!--(.|\s)*?-->", "")
        strOutput = Regex.Replace(strOutput, "&", " ")
        strOutput = Regex.Replace(strOutput, "<script.*?</script>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        strOutput = Regex.Replace(strOutput, "<style.*?</style>", "", RegexOptions.Singleline Or RegexOptions.IgnoreCase)

        Dim httpInt As Integer '' https://
        Dim tempLink As String
        Dim i As Integer
        Dim lastTop As Integer
        ''Dim i As Integer = 0
        '' Loop until amount of requests is equal to the amount of requests to get
        ''txtTestScrape.Text = ""
        For i = 1 To numResultToGet Step 1
            httpInt = strOutput.IndexOf("/url?q")
            strOutput = strOutput.Substring(httpInt)

            tempLink = strOutput.Substring(strOutput.IndexOf("h"), strOutput.IndexOf(" "))
            linksArray(i) = tempLink.Substring(tempLink.IndexOf("h"), tempLink.IndexOf(" "))

            '' Create labels or buttons to click on
            Dim lblLinks As New Label
            lblLinks.Name = "lblLinks" + i.ToString()
            lblLinks.Text = linksArray(i) '' Substring to only show the Main link ex: minecraft.net
            lblLinks.AutoSize = True
            lblLinks.Location = New System.Drawing.Point(0, lastTop + 20)
            AddHandler lblLinks.Click, AddressOf lblLinks_Click
            pnlResults.Controls.Add(lblLinks)

            lastTop = lblLinks.Top + lblLinks.Height

            '' Testing
            strOutput = strOutput.Substring(strOutput.IndexOf(" "))
            ''txtTestScrape.Text = txtTestScrape.Text + linksArray(i) + vbNewLine
        Next
    End Sub

    Private Sub lblLinks_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        '' Use this to open the page in the web browser
        Dim lblClicked As Label = DirectCast(sender, Label)
        wbbResultView.Visible = True
        btnCloseBrowser.Visible() = True
        wbbResultView.Url = New Uri(lblClicked.Text)
    End Sub

    Private Sub radThree_CheckedChanged(sender As Object, e As EventArgs) Handles radThree.CheckedChanged
        '' Sets the amount of results to get to 3
        numResultToGet = 3
    End Sub

    Private Sub radFive_CheckedChanged(sender As Object, e As EventArgs) Handles radFive.CheckedChanged
        '' Sets the amount of results to get to 5
        numResultToGet = 5
    End Sub

    Private Sub radTen_CheckedChanged(sender As Object, e As EventArgs) Handles radTen.CheckedChanged
        '' Sets the amount of results to get to 10
        numResultToGet = 10
    End Sub

    Private Sub radFifteen_CheckedChanged(sender As Object, e As EventArgs) Handles radFifteen.CheckedChanged
        '' Sets the amount of results to get to 15
        numResultToGet = 15
    End Sub

    Private Sub btnSearchBar_Click(sender As Object, e As EventArgs) Handles btnSearchBar.Click
        '' Same as clicking the icon
        lblSearchIcon_Click(sender, e)
    End Sub

    Private Sub btnCloseBrowser_Click(sender As Object, e As EventArgs) Handles btnCloseBrowser.Click
        '' Closes the browser
        btnCloseBrowser.Visible = False
        wbbResultView.Visible = False
    End Sub

    Private Sub btnPost_Click(sender As Object, e As EventArgs) Handles btnPost.Click
        '' Opens up the Post/answer/View page
        pnlPostAnswer.Visible() = True
        pnlSearch.Visible() = False
        pnlDeDuck.Visible() = False
        pnlViewPost.Visible() = False
        pnlProfile.Visible() = False
        pnlSettings.Visible() = False

        btnDeDuck.BackColor = Color.FromArgb(138, 138, 138)
        btnSearch.BackColor = Color.FromArgb(138, 138, 138)
        btnPost.BackColor = Color.FromArgb(240, 153, 48)
        btnProfile.BackColor = Color.FromArgb(138, 138, 138)
        btnSettings.BackColor = Color.FromArgb(138, 138, 138)

        lblPageTitle.Text = "Post A Question"
        lblPost.BackColor = Color.FromArgb(240, 153, 48)
        lblView.BackColor = Color.FromArgb(138, 138, 138)
        txtTitle.Select()
        ClearPosts()
        ClearAnswers()
    End Sub

    Private Sub lblView_Click(sender As Object, e As EventArgs) Handles lblView.Click
        '' Open the view panel
        lblPost.BackColor = Color.FromArgb(138, 138, 138)
        lblView.BackColor = Color.FromArgb(240, 153, 48)
        lblPageTitle.Text = "View Questions"

        pnlViewPost.Visible() = True
        txtSearchPosts.Select()
        ClearPosts()
        ClearAnswers()
    End Sub

    Private Sub lblPost_Click(sender As Object, e As EventArgs) Handles lblPost.Click
        '' Close the view and answer panels and open the post question panel
        lblPageTitle.Text = "Post A Question"
        lblPost.BackColor = Color.FromArgb(240, 153, 48)
        lblView.BackColor = Color.FromArgb(138, 138, 138)

        pnlPostAnswer.Visible() = True
        pnlViewPost.Visible() = False
        txtTitle.Select()
        txtSearchPosts.Text = ""
        ClearPosts()
        ClearAnswers()
        ' Remove all the labels from the search
        'txtTestResults.Text = ""
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        '' Clears Post form
        txtTitle.Text = ""
        rTxtDescribe.Text = ""
        txtTags.Text = ""
        txtCodeBox.Text = ""
        txtTitle.Select()
    End Sub

    Private Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        '' Sends the information to the database and then calls btnCancel_Click (clears form)
        '' Get the information from the fields
        Dim title As String = txtTitle.Text
        Dim tags As String = txtTags.Text
        Dim description As String = rTxtDescribe.Text
        Dim code As String = txtCodeBox.Text
        Dim testUsername As String = lblProfileName.Text

        '' Making the connection
        Dim querry As String
        querry = "INSERT INTO `test_posts`(`username`, `title`, `description`, `code`, `tags`) VALUES ('" + testUsername + "', '" + title + "', '" + description + "', '" + code + "', '" + tags + "');"
        myConn.Open()

        If (myConn.State = ConnectionState.Open) Then
            '' Error Trapping
            If (txtTitle.Text.Contains(" ") And txtTags.Text.Contains(",") And rTxtDescribe.Text.Length >= 100 And txtCodeBox.Text.Length >= 50) Then
                '' Command to send information to the database to be stored
                myCmd.Connection = myConn
                myCmd.CommandText = querry
                If myCmd.ExecuteNonQuery() = 1 Then
                    MessageBox.Show("Seccessfully posted your question. You can view it under your profile or through a search.")
                Else
                    MessageBox.Show("Server is down. Contact Support with error code 800189-SD")
                End If
            Else
                '' Use a case clause and check for each error tarapping condition. Display message accordingly
                Dim displayMessage As String = ""
                If (Not txtTitle.Text.Contains(" ")) Then
                    displayMessage = displayMessage + "Make sure your title is longer than 3 words. Make it descriptive and distinct." & vbNewLine
                End If

                If (Not txtTags.Text.Contains(",")) Then
                    displayMessage = displayMessage + "Please make sure to have at least two tags seperated by a comma." & vbNewLine
                End If

                If (Not rTxtDescribe.Text.Length >= 100) Then
                    displayMessage = displayMessage + "You must have at least 100 characters in your description. Clearly explain your problem." & vbNewLine
                End If

                If (Not txtCodeBox.Text.Length >= 50) Then
                    displayMessage = displayMessage + "You must also have at least 50 characters in the code section. Make sure you give enough code to work out the problem." & vbNewLine
                End If

                ''Display message to add missing information
                MessageBox.Show(displayMessage)
            End If
        Else
            MessageBox.Show("Server is down. Contact Support with error code 800391-SD")
        End If

        myConn.Close()

        '' Clear all fields and auto focus
        Call btnCancel_Click(sender, e)
    End Sub

    Private Sub btnSearchPosts_Click(sender As Object, e As EventArgs) Handles btnSearchPosts.Click
        '' Searches the database using the tags and text in the search bar. (title and tags)
        ClearPosts()

        ' Searching stuff
        Dim search As String = txtSearchPosts.Text
        ' Making the connection
        Dim querry As String
        querry = "SELECT username, title, description, code, tags FROM test_posts WHERE username = '" + search + "' OR title = '" + search + "'OR tags = '" + search + "' 
        OR title LIKE '%" + search + "%' OR username LIKE '%" + search + "%' OR tags LIKE '%" + search + "%';" ' OR another for the tags as well (this is will look for similar as well)

        myConn.Open() ' Opens the connection

        If (myConn.State = ConnectionState.Open) Then
            ' Check to see if the querry was successful or not. If not then show results that contain the search words split by spaces. Ones with more in common first
            ' Command sent to the server to retrieve information
            myCmd.Connection = myConn
            myCmd.CommandText = querry

            ' Reading and displaying information
            'Dim lrd As MySqlDataReader = myCmd.ExecuteReader()
            Dim dataAdapter As New MySqlDataAdapter
            dataAdapter.SelectCommand = myCmd
            Dim ds As New DataTable
            dataAdapter.Fill(ds)

            ' This should do each row seperatly
            Dim i As Integer = 0
            Dim lastTop As Integer

            For Each row As DataRow In ds.Rows

                ReDim Preserve usernameArray(ds.Rows.Count() + 1)
                ReDim Preserve titleArray(ds.Rows.Count() + 1)

                i += 1

                Dim descriptionSub As String
                If (row("description").ToString().Length >= 101) Then
                    descriptionSub = row("description").ToString().Substring(0, 101)
                Else
                    descriptionSub = row("description").ToString()
                End If

                usernameArray(i) = row("username").ToString()
                titleArray(i) = row("title").ToString()

                '' Create labels or buttons to click on
                Dim lblPost As New Label
                lblPost.Name = "lblPost " + i.ToString()
                lblPost.Text = "Title: " + row("title").ToString() + " By: " + row("username").ToString() & vbNewLine + "Short Description: " & vbNewLine + descriptionSub + "..." & vbNewLine + "Tags: " + row("tags").ToString()
                lblPost.AutoSize = True
                lblPost.Location = New System.Drawing.Point(20, lastTop + 20)
                AddHandler lblPost.Click, AddressOf lblPosts_Click
                pnlPostResults.Controls.Add(lblPost)

                lastTop = lblPost.Top + lblPost.Height
            Next
            numberPosts = i
            ' Close the connection after
            myConn.Close()
        Else
            MessageBox.Show("Server is down. Contact Support with error code 800391-SD")
        End If
    End Sub

    Private Sub lblPosts_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        ' Open a panel with the respective information about the post
        Dim postLabel As New Label
        postLabel = DirectCast(sender, Label)

        Dim i As Integer
        Dim labelName As String = postLabel.Name
        Dim charNumbers As String = ""
        Dim myChars() As Char = labelName.ToCharArray()
        For Each ch As Char In myChars
            If Char.IsDigit(ch) Then
                charNumbers += ch
            End If
        Next

        i = CInt(charNumbers)

        Dim username As String = usernameArray(i)
        Dim title As String = titleArray(i)

        pnlPostView.Visible() = True

        'Debug purpose
        'MessageBox.Show("Open: " + title + " User: " + username)

        ' Get the data from the database to display
        Dim querry As String
        querry = "SELECT username, title, description, code, tags FROM test_posts WHERE title = '" + title + "' AND username = '" + username + "';" ' OR another for the tags as well (this is will look for similar as well)
        myCmd.CommandText = querry

        myConn.Open() ' Opens the connection

        Dim lrd As MySqlDataReader = myCmd.ExecuteReader()
        lrd.Read()

        If lrd.HasRows Then
            lblViewPostTitle.Text = "Title: " + lrd("title").ToString()
            lblViewPostBy.Text = "By: " + lrd("username").ToString()
            lblViewPostTags.Text = "Tags: " + lrd("tags").ToString()
            rTxtViewPostDescription.Text = lrd("description").ToString()
            txtViewPostCode.Text = lrd("code").ToString()
        End If

        lrd.Close()
        myConn.Close()

        ' Check to see if there are any answers and display them below the create answer button. If there arent then dont do anything
        ClearAnswers()
        Dim querryAnswer As String
        querryAnswer = "SELECT title, postUsername, answerUsername, answerCode, answerDescription FROM test_answers WHERE title = '" + lblViewPostTitle.Text + "' AND postUsername = '" + lblViewPostBy.Text + "';"
        myCmd.CommandText = querryAnswer

        myConn.Open() ' Opens the connection

        If (myConn.State = ConnectionState.Open) Then
            ' Check to see if the querry was successful or not. If not then show results that contain the search words split by spaces. Ones with more in common first
            ' Command sent to the server to retrieve information
            myCmd.Connection = myConn

            ' Reading and displaying information
            'Dim lrd As MySqlDataReader = myCmd.ExecuteReader()
            Dim dataAdapter As New MySqlDataAdapter
            dataAdapter.SelectCommand = myCmd
            Dim ds As New DataTable
            dataAdapter.Fill(ds)

            If ds.Rows.Count() > 0 Then

                ' This should do each row seperatly
                Dim iA As Integer = 0
                Dim lastTop As Integer = btnCreateAnswer.Location.Y + 20

                For Each row As DataRow In ds.Rows

                    iA += 1

                    '' Create labels or buttons to click on
                    Dim lblAnswer As New Label
                    lblAnswer.Name = "lblAnswer " + iA.ToString()
                    lblAnswer.Text = row("postUsername").ToString() & vbNewLine & vbNewLine + "Answer Description: " & vbNewLine + row("answerDescription").ToString() & vbNewLine & vbNewLine + "Answer Code: " & vbNewLine + row("answerCode").ToString()
                    lblAnswer.AutoSize = True
                    lblAnswer.Location = New System.Drawing.Point(20, lastTop + 20)
                    pnlPostView.Controls.Add(lblAnswer)

                    lastTop = lblAnswer.Top + lblAnswer.Height
                    lastTop = lblAnswer.Top + lblAnswer.Height
                Next

                numberAnswers = iA
            End If

            ' Close the connection after
            myConn.Close()
        Else
                MessageBox.Show("Server is down. Contact Support with error code 800391-SD")
        End If
    End Sub

    Private Sub ClearPosts()
        ' Clears all labels of the previous search in posts if any
        While (Not numberPosts.Equals(0))
            pnlPostResults.Controls.RemoveByKey("lblPost " + numberPosts.ToString())
            numberPosts -= 1
        End While
    End Sub

    Private Sub ClearAnswers()
        ' Clears all labels of the previous search in posts if any
        While (Not numberAnswers.Equals(0))
            pnlPostView.Controls.RemoveByKey("lblAnswer " + numberAnswers.ToString())
            numberAnswers -= 1
        End While
    End Sub

    Private Sub btnCloseViewPost_Click(sender As Object, e As EventArgs) Handles btnCloseViewPost.Click
        '' Close the post view
        If (pnlCreateAnswer.Visible.Equals(True)) Then
            btnAnswerCancel_Click(sender, e)
        End If

        pnlPostView.Visible() = False
        ClearAnswers()
    End Sub

    Private Sub btnCreateAnswer_Click(sender As Object, e As EventArgs) Handles btnCreateAnswer.Click
        '' Allows the user to make an answer
        pnlCreateAnswer.Visible() = True
        rTxtAnswerDescription.Focus()
    End Sub

    Private Sub btnAnswerCancel_Click(sender As Object, e As EventArgs) Handles btnAnswerCancel.Click
        ' Clear the answer form and turn it invisable
        rTxtAnswerDescription.Text = ""
        txtAnswerCode.Text = ""
        pnlCreateAnswer.Visible() = False
    End Sub

    Private Sub btnSubmitAnswer_Click(sender As Object, e As EventArgs) Handles btnSubmitAnswer.Click
        ' Submit the information to the database

        Dim postUsername As String = lblViewPostBy.Text
        Dim title As String = lblViewPostTitle.Text
        Dim description As String = rTxtAnswerDescription.Text
        Dim code As String = txtAnswerCode.Text
        Dim answerUsername As String = lblProfileName.Text

        If (myConn.State = ConnectionState.Open) Then
            myConn.Close()
        End If

        '' Making the connection
        Dim querry As String
        querry = "INSERT INTO `test_answers`(`title`, `postUsername`, `answerUsername`, `answerCode`, `answerDescription`) VALUES ('" + title + "', '" + postUsername + "', '" + answerUsername + "', '" + code + "', '" + description + "');"
        myConn.Open()

        If (myConn.State = ConnectionState.Open) Then
            '' Error Trapping
            If (rTxtAnswerDescription.Text.Length >= 50 And txtAnswerCode.Text.Length >= 25) Then
                '' Command to send information to the database to be stored
                myCmd.Connection = myConn
                myCmd.CommandText = querry
                If myCmd.ExecuteNonQuery() = 1 Then
                    MessageBox.Show("Seccessfully posted your answer. You can view it under your profile or close and reopen the post.")
                Else
                    MessageBox.Show("Server is down. Contact Support with error code 800189-SD")
                End If
            Else
                '' Use a case clause and check for each error tarapping condition. Display message accordingly
                Dim displayMessage As String = ""

                If (Not rTxtAnswerDescription.Text.Length >= 50) Then
                    displayMessage = displayMessage + "You must have at least 50 characters in your description. Clearly explain your problem." & vbNewLine
                End If

                If (Not txtAnswerCode.Text.Length >= 25) Then
                    displayMessage = displayMessage + "You must also have at least 25 characters in the code section. Make sure you give enough code to work out the problem." & vbNewLine
                End If

                ''Display message to add missing information
                MessageBox.Show(displayMessage)
            End If
            myConn.Close()
        Else
            MessageBox.Show("Server is down. Contact Support with error code 800667-SD")
        End If

        btnAnswerCancel_Click(sender, e)
    End Sub

    Private Sub btnProfile_Click(sender As Object, e As EventArgs) Handles btnProfile.Click
        '' Opens the profile page and closes all other pages
        '' Hides all pages except for the de ducking page
        btnDeDuck.BackColor = Color.FromArgb(138, 138, 138)
        btnSearch.BackColor = Color.FromArgb(138, 138, 138)
        btnPost.BackColor = Color.FromArgb(138, 138, 138)
        btnProfile.BackColor = Color.FromArgb(62, 117, 207)
        btnSettings.BackColor = Color.FromArgb(138, 138, 138)

        pnlSearch.Visible() = False
        pnlDeDuck.Visible() = False
        pnlPostAnswer.Visible() = False
        pnlViewPost.Visible() = False
        pnlProfile.Visible() = True
        pnlSettings.Visible() = False

        lblPageTitle.Text = "   Profile"
        ''txtTestScrape.Text = ""
        ClearPosts()
        ClearAnswers()

        GetProfileInformation()
        '' Currently not doing this but plan to in the future
        ''GetUserQAndA()
    End Sub

    Private Sub GetProfileInformation()
        '' Is called when the profile button is clicked. It gets the user information from the database
        '' Database stuff
        Dim querryProfle As String
        querryProfle = "SELECT name, bio, profile_pic, username FROM test_profile WHERE username = '" + lblProfileName.Text + "';"
        myCmd.CommandText = querryProfle
        myCmd.Connection = myConn

        myConn.Open()

        Dim lrd As MySqlDataReader = myCmd.ExecuteReader()
        lrd.Read()

        If (myConn.State = ConnectionState.Open) Then
            If lrd.HasRows Then
                txtCustomerName.Text = lrd("name").ToString()
                txtProfileBio.Text = lrd("bio").ToString()
                txtProfilePageName.Text = lrd("username").ToString()
                lblProfileName.Text = lrd("username").ToString()

                '' Setting the Image
                Dim mstream As MemoryStream = New MemoryStream(CType(lrd("profile_pic"), Byte()))
                Dim returnImage As Image = Image.FromStream(mstream)
                picProfilePage.Image = returnImage
                picProfile.Image = picProfilePage.Image
            End If

            lrd.Close()
            myConn.Close()
        Else
            MessageBox.Show("Server is down. Contact Support with error code 800667-SD")
        End If
    End Sub

    Private Sub GetUserQAndA()
        '' Is called when the profile button is clicked. It gets the users questions and answers from the database
    End Sub

    Private Sub btnEditProfile_Click(sender As Object, e As EventArgs) Handles btnEditProfile.Click
        '' Allows the user to change the bio and customer name
        btnDeDuck.Enabled() = False
        btnSearch.Enabled() = False
        btnPost.Enabled() = False
        btnProfile.Enabled() = False
        btnSettings.Enabled() = False

        btnSaveProfile.Enabled = True
        btnUpload.Enabled() = True
        btnEditProfile.Enabled() = False

        txtCustomerName.Enabled() = True
        txtProfileBio.Enabled() = True
    End Sub

    Private Sub btnSaveProfile_Click(sender As Object, e As EventArgs) Handles btnSaveProfile.Click
        '' Saves the changes of the profile to the database
        '' Turn the image to a blob
        Dim mstream As New MemoryStream()
        picProfilePage.Image.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg)
        arrImage = mstream.GetBuffer()
        Dim fileSize As UInt32
        fileSize = CUInt(mstream.Length)

        mstream.Close()

        '' Database
        Dim querrySave As String
        querrySave = "UPDATE test_profile SET name = @pname, bio = @pbio, profile_pic = @parrImage WHERE username = '" + lblProfileName.Text + "';"
        myCmd.CommandText = querrySave
        myCmd.Parameters.AddWithValue("@pname", txtCustomerName.Text)
        myCmd.Parameters.AddWithValue("@pbio", txtProfileBio.Text)
        myCmd.Parameters.AddWithValue("@parrImage", arrImage)

        myConn.Open()

        myCmd.Connection = myConn

        If (myConn.State = ConnectionState.Open) Then
            myCmd.ExecuteNonQuery()

            btnDeDuck.Enabled() = True
            btnSearch.Enabled() = True
            btnPost.Enabled() = True
            btnProfile.Enabled() = True
            btnSettings.Enabled() = True

            btnSaveProfile.Enabled = False
            btnUpload.Enabled() = False
            btnEditProfile.Enabled() = True

            txtCustomerName.Enabled() = False
            txtProfileBio.Enabled() = False

            myConn.Close()
        Else
            MessageBox.Show("Server is down. Contact Support with error code 800667-SD")
        End If

        GetProfileInformation()
    End Sub

    Private Sub btnUpload_Click(sender As Object, e As EventArgs) Handles btnUpload.Click
        '' Lets the user choose a file to upload as their profile picture
        diaOpenPicture = New OpenFileDialog()

        diaOpenPicture.Filter = "Image File (*.jpg;*.bmp;*.gif;*.png)|*.jpg;*.bmp;*.gif;*.png"

        If (diaOpenPicture.ShowDialog() = DialogResult.OK) Then
            picturePath = diaOpenPicture.FileName
            picProfilePage.ImageLocation = picturePath
            picProfile.Image = picProfilePage.Image
        ElseIf (diaOpenPicture.ShowDialog() = DialogResult.Cancel) Then
            '' picturePath = '' Path of default picture
        End If
    End Sub

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        '' Hides all pages except for the settings page
        btnDeDuck.BackColor = Color.FromArgb(138, 138, 138)
        btnSearch.BackColor = Color.FromArgb(138, 138, 138)
        btnPost.BackColor = Color.FromArgb(138, 138, 138)
        btnProfile.BackColor = Color.FromArgb(138, 138, 138)
        btnSettings.BackColor = Color.FromArgb(54, 161, 18)

        pnlSearch.Visible() = False
        pnlDeDuck.Visible() = False
        pnlPostAnswer.Visible() = False
        pnlViewPost.Visible() = False
        pnlProfile.Visible() = False
        pnlSettings.Visible() = True

        lblPageTitle.Text = "Settings"
        ''txtTestScrape.Text = ""
        ClearPosts()
        ClearAnswers()
    End Sub

    Private Sub ReadResourcesFile(fileName As String)
        '' Reads the file and creates a label to display respective policy
        Dim fileNamePath As String = "resources/" + fileName
        Dim line As String = ""

        If (File.Exists(fileName)) Then
            Dim objReader As New StreamReader(fileName)
            Dim i As Integer = 0

            Do While (objReader.Peek() <> -1)
                i = i + 1
                line = objReader.ReadLine()
                tips(i) = line
            Loop
            numberOfTips = i
        Else
            MessageBox.Show("File does not exist. Contact Support with error code 800932-MF and 800673-MF")
        End If
    End Sub

    Private Sub ReadFile(fileName As String)
        '' Reads the file and creates a label to display respective policy
        Dim fileNamePath As String = "resources/" + fileName
        Dim line As String = ""

        If (File.Exists(fileName)) Then
            Dim objReader As New StreamReader(fileName)
            Dim i As Integer = 0

            Do While (objReader.Peek() <> -1)
                i = i + 1
                line = line + objReader.ReadLine() + vbNewLine
            Loop
        Else
            MessageBox.Show("File does not exist. Contact Support with error code 800932-MF and 800673-MF")
        End If

        fileText = line
    End Sub

    Private Sub lblPrivacy_Click(sender As Object, e As EventArgs) Handles lblPrivacy.Click
        '' This will show the privacy policy
        Dim fileName As String = "PrivacyPolicy.txt"
        ReadFile(fileName)

        Static clicked As Boolean = False
        Dim lblPolicy As New Label

        If (clicked.Equals(True)) Then
            '' Remove Label if clicked
            pnlSettings.Controls.RemoveByKey("privacy")

            lblPrivacy.Text = "Privacy Policy ^"

            clicked = False
        ElseIf (clicked.Equals(False)) Then
            Dim lastTop As Integer = lblTerms.Location.Y

            '' Create label on click
            lblPolicy.Name = "privacy"
            lblPolicy.Text = fileText
            lblPolicy.AutoSize = True
            lblPolicy.Location = New System.Drawing.Point(20, lastTop + 20)
            pnlSettings.Controls.Add(lblPolicy)

            lblPrivacy.Text = "Privacy Policy >"

            clicked = True
        End If
    End Sub

    Private Sub lblTerms_Click(sender As Object, e As EventArgs) Handles lblTerms.Click
        '' This will show the terms and conditions
        Dim fileName As String = "TermsAndConditions.txt"
        ReadFile(fileName)

        Static clicked As Boolean = False
        Dim lblTermsConditions As New Label

        If (clicked.Equals(True)) Then
            '' Remove Label if clicked
            pnlSettings.Controls.RemoveByKey("terms")

            lblTerms.Text = "Terms and Conditions ^"

            clicked = False
        ElseIf (clicked.Equals(False)) Then
            Dim lastTop As Integer = lblTerms.Location.Y

            '' Create label on click
            lblTermsConditions.Name = "terms"
            lblTermsConditions.Text = fileText
            lblTermsConditions.AutoSize = True
            lblTermsConditions.Location = New System.Drawing.Point(20, lastTop + 20)
            pnlSettings.Controls.Add(lblTermsConditions)

            lblTerms.Text = "Terms and Conditions >"

            clicked = True
        End If
    End Sub

    Private Sub btnUpdatePassword_Click(sender As Object, e As EventArgs) Handles btnUpdatePassword.Click
        '' This will update the database with the new password
        If (txtNewPassword.Text.Equals(txtConfirmPassword.Text)) Then
            '' Database
            Dim querrySave As String
            querrySave = "UPDATE test_profile SET password = @password WHERE username = '" + lblProfileName.Text + "' AND password = '" + txtCurrentPassword.Text + "';"
            myCmd.CommandText = querrySave
            myCmd.Parameters.AddWithValue("@password", txtNewPassword.Text)

            myConn.Open()

            myCmd.Connection = myConn

            If (myConn.State = ConnectionState.Open) Then
                myCmd.ExecuteNonQuery()

                myConn.Close()
                MessageBox.Show("You have changed your password. Congrats!")
                txtCurrentPassword.Text = ""
                txtNewPassword.Text = ""
                txtConfirmPassword.Text = ""
            Else
                MessageBox.Show("Server is down. Contact Support with error code 800667-SD")
            End If
        End If
    End Sub

    Private Sub btnUpdateEmail_Click(sender As Object, e As EventArgs) Handles btnUpdateEmail.Click
        '' This will update the database with the new email
        '' Database
        Dim querrySave As String
        querrySave = "UPDATE test_profile SET email = @email WHERE username = '" + lblProfileName.Text + "' AND email = '" + txtCurrentEmail.Text + "';"
        myCmd.CommandText = querrySave
        myCmd.Parameters.AddWithValue("@email", txtNewEmail.Text)

        myConn.Open()

        myCmd.Connection = myConn

        If (myConn.State = ConnectionState.Open) Then
            myCmd.ExecuteNonQuery()

            myConn.Close()
            MessageBox.Show("You have changed your password. Congrats!")
            txtCurrentEmail.Text = ""
            txtNewEmail.Text = ""
        Else
            MessageBox.Show("Server is down. Contact Support with error code 800667-SD")
        End If
    End Sub

    Private Sub radLight_CheckedChanged(sender As Object, e As EventArgs) Handles radLight.CheckedChanged
        '' Changes the theme of the application to light 153, 153, 153
        Panel1.BackColor = Color.FromArgb(153, 153, 153)
        pnlCreateAnswer.BackColor = Color.FromArgb(153, 153, 153)
        pnlDeDuck.BackColor = Color.FromArgb(153, 153, 153)
        pnlPost.BackColor = Color.FromArgb(153, 153, 153)
        pnlPostAnswer.BackColor = Color.FromArgb(153, 153, 153)
        pnlPostResults.BackColor = Color.FromArgb(153, 153, 153)
        pnlPostView.BackColor = Color.FromArgb(153, 153, 153)
        pnlProfile.BackColor = Color.FromArgb(153, 153, 153)
        pnlResults.BackColor = Color.FromArgb(153, 153, 153)
        pnlSearch.BackColor = Color.FromArgb(153, 153, 153)
        pnlSettings.BackColor = Color.FromArgb(153, 153, 153)
        pnlViewPost.BackColor = Color.FromArgb(153, 153, 153)
        pnlTopBanner.BackColor = Color.FromArgb(153, 153, 153)

        Panel2.BackColor = Color.FromArgb(87, 87, 87)
        Panel3.BackColor = Color.FromArgb(87, 87, 87)
        Panel4.BackColor = Color.FromArgb(87, 87, 87)

    End Sub

    Private Sub radDark_CheckedChanged(sender As Object, e As EventArgs) Handles radDark.CheckedChanged
        '' Changes the theme of the application to dark
        Panel1.BackColor = Color.FromArgb(87, 87, 87)
        pnlCreateAnswer.BackColor = Color.FromArgb(87, 87, 87)
        pnlDeDuck.BackColor = Color.FromArgb(87, 87, 87)
        pnlPost.BackColor = Color.FromArgb(87, 87, 87)
        pnlPostAnswer.BackColor = Color.FromArgb(87, 87, 87)
        pnlPostResults.BackColor = Color.FromArgb(87, 87, 87)
        pnlPostView.BackColor = Color.FromArgb(87, 87, 87)
        pnlProfile.BackColor = Color.FromArgb(87, 87, 87)
        pnlResults.BackColor = Color.FromArgb(87, 87, 87)
        pnlSearch.BackColor = Color.FromArgb(87, 87, 87)
        pnlSettings.BackColor = Color.FromArgb(87, 87, 87)
        pnlViewPost.BackColor = Color.FromArgb(87, 87, 87)
        pnlTopBanner.BackColor = Color.FromArgb(87, 87, 87)

        Panel2.BackColor = Color.FromArgb(153, 153, 153)
        Panel3.BackColor = Color.FromArgb(153, 153, 153)
        Panel4.BackColor = Color.FromArgb(153, 153, 153)

    End Sub

    Private Sub lblAccount_Click(sender As Object, e As EventArgs) Handles lblAccount.Click
        '' Opens the login page
        pnlLoginPage.Visible() = True
        pnlSignupForm.Visible() = False

        txtLoginUsername.Focus()
    End Sub

    Private Sub btnSignup_Click(sender As Object, e As EventArgs) Handles btnSignup.Click
        '' Enters the user to the database and creates a profile if username is not taken
        If (txtSignupName.Text.Length > 8 And txtSignupUsername.Text.Length > 8 And txtSignupEmail.Text.Length > 10 And txtSignupEmail.Text.Contains("@") And txtSignupPassword.Text.Equals(txtSignupConfirm.Text)) Then
            '' Turn the image to a blob
            Dim mstream As New MemoryStream()
            picProfile.Image.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg)
            arrImage = mstream.GetBuffer()
            Dim fileSize As UInt32
            fileSize = CUInt(mstream.Length)

            mstream.Close()

            '' Database
            Dim querrySave As String
            querrySave = "INSERT INTO test_profile SET `name` = @name, `bio` = @bio, `profile_pic` = @arrImage, `username` = @username, `email` = @email, `password` = @password;"
            myCmd.CommandText = querrySave
            myCmd.Parameters.AddWithValue("@name", txtSignupName.Text)
            myCmd.Parameters.AddWithValue("@bio", "Edit your profile to change your bio.")
            myCmd.Parameters.AddWithValue("@arrImage", arrImage)
            myCmd.Parameters.AddWithValue("@username", txtSignupUsername.Text)
            myCmd.Parameters.AddWithValue("@email", txtSignupEmail.Text)
            myCmd.Parameters.AddWithValue("@password", txtSignupPassword.Text)

            myConn.Open()

            myCmd.Connection = myConn

            If (myConn.State = ConnectionState.Open) Then
                myCmd.ExecuteNonQuery()

                pnlLoginPage.Visible() = True
                pnlSignupForm.Visible() = False

                lblProfileName.Text = txtSignupUsername.Text
                txtLoginUsername.Text = txtSignupUsername.Text
                txtLoginPassword.Focus()

                myConn.Close()
            Else
                MessageBox.Show("Server is down. Contact Support with error code 800667-SD")
            End If
        Else
            Dim errorMessage As String = ""

            If (txtSignupName.Text.Length < 8) Then
                errorMessage += "Make sure your put your last and first name." & vbNewLine
            End If

            If (txtSignupUsername.Text.Length < 8) Then
                errorMessage += "Make sure your username is longer than 8 characters." & vbNewLine
            End If

            If (Not txtSignupEmail.Text.Length > 10 And Not txtSignupEmail.Text.Contains("@")) Then
                errorMessage += "Make sure your email is longer than 10 characters and contains an @ symbol." & vbNewLine
            End If

            If (Not txtSignupPassword.Text.Equals(txtSignupConfirm.Text)) Then
                errorMessage += "Make sure both passwords match." & vbNewLine
            End If

            MessageBox.Show(errorMessage)
        End If
    End Sub

    Private Sub lblCreate_Click(sender As Object, e As EventArgs) Handles lblCreate.Click
        '' Opens the Sign up page
        pnlLoginPage.Visible() = False
        pnlSignupForm.Visible() = True

        txtSignupName.Focus()
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        '' Allows the enter to use their account information to access the app

        '' Database
        Dim querrySave As String
        querrySave = "SELECT `name`, `bio`, `profile_pic`, `username`, `email`, `password` FROM `test_profile` WHERE username = '" + txtLoginUsername.Text + "' AND password = '" + txtLoginPassword.Text + "';"
        myCmd.CommandText = querrySave
        myCmd.Connection = myConn

        myConn.Open()

        Dim lrd As MySqlDataReader = myCmd.ExecuteReader()
        lrd.Read()

        If (myConn.State = ConnectionState.Open) Then

            If lrd.HasRows Then
                lblProfileName.Text = lrd("username").ToString()

                '' Setting the Image
                Dim mstream As MemoryStream = New MemoryStream(CType(lrd("profile_pic"), Byte()))
                Dim returnImage As Image = Image.FromStream(mstream)
                picProfilePage.Image = returnImage
                picProfile.Image = picProfilePage.Image

                pnlSignup.Visible() = False
            Else
                MessageBox.Show("Please use the correct username and password.")
                txtLoginUsername.Text = ""
                txtLoginPassword.Text = ""
            End If

            lrd.Close()
            myConn.Close()
        Else
            MessageBox.Show("Server is down or try again. Contact Support with error code 800667-SD")
        End If
    End Sub
End Class