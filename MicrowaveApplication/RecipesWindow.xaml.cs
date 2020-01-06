using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicrowaveApplication
{
    //Event handler to update RecipesWindow's recipe ComboBox.
    public delegate void RecipeUpdateHandler();
    /// <summary>
    /// Interaction logic for RecipesWindow.xaml.
    /// </summary>

    public partial class RecipesWindow
    {
        public event RecipeUpdateHandler RecipeUpdate;

        public RecipesWindow()
        {
            InitializeComponent();
            FillRecipesBar();
        }

        // Fill the ComboBox with all recipes.
        private void FillRecipesBar()
        {
            using (StreamReader r = new StreamReader("recipes.json"))
            {
                cbRecipe.Items.Clear();
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                //Convert the read JSON file into an array so each element can be added to the ComboBox.
                foreach (var item in array)
                {
                    cbRecipe.Items.Add(item.name);
                }
            }
        }

        //Show description of a recipe when it's selected.
        private void CbRecipe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = "something went wrong";
            using (var r = new StreamReader("recipes.json"))
            {
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                foreach (var item in array)
                {
                    //Scan trough the JSON elements and write a description based on the selected one.
                    if (cbRecipe.SelectedItem != null && item.name == cbRecipe.SelectedItem.ToString())
                    {
                        text =
                            $"Recipe id {item.id} \nRecipe for {item.name} \nRecommended preparation time is {item.time}";
                    }
                }
            }

            recipeDesc.Text = text;
        }

        //Add a new recipe to the JSON storage.
        private void AddRecipe(object sender, RoutedEventArgs e)
        {
            //Get the user inputs.
            string recipeName = RecipeNameInput.Text;
            string recipeTime = RecipeTimeInput.Text;

            string jsonToOutput;
            using (var r = new StreamReader("recipes.json"))
            {
                //Read the JSON file and convert it to an array.
                string json = r.ReadToEnd();
                JsonConvert.DeserializeObject(json);
                var jsonArray = JArray.Parse(json);

                //Create a new JSON element which is created from the users input and add save the file again.
                var newRecipe = new JObject {["name"] = recipeName, ["time"] = recipeTime};
                jsonArray.Add(newRecipe);
                jsonToOutput = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);


            }

            File.WriteAllText($"{Directory.GetCurrentDirectory()}/recipes.json", jsonToOutput);
            FillRecipesBar();

            //Call an event so the MainWindow dropdown can update its recipes.
            RecipeUpdate?.Invoke();
            

            Close();
        }

        //Remove a JSON file from storage.
        private void RemoveRecipe(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            string jsonToOutput;
            using (var r = new StreamReader("recipes.json"))
            {
                //Read the JSON file and convert it to an array.
                var id = 0;
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);

                //Get the number of the selected recipe.
                foreach (var item in array)
                {
                    if (item.name == cbRecipe.SelectedItem.ToString())
                    {
                        id = counter;
                    }
                    counter++;
                }

                cbRecipe.SelectedItem = 1;

                //Convert to array list, remove the selected recipe, and convert it back to json so it can be saved.
                ArrayList arrLst = new ArrayList(array);
                arrLst.RemoveAt(id);
                dynamic editedArray = arrLst.ToArray();
                jsonToOutput = JsonConvert.SerializeObject(editedArray, Formatting.Indented);


            }
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/recipes.json", jsonToOutput);
            FillRecipesBar();

            //Call an event so the MainWindow dropdown can update its recipes.
            RecipeUpdate?.Invoke();

            Close();
        }
    }
}
