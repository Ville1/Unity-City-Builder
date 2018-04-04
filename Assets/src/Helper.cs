using System.Text;

public class Helper {

    /// <summary>
    /// Replaces underscores with whitespaces and capitalizes text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Parse_To_Human_Readable(string text)
    {
        text = text.Replace('_', ' ').Trim().ToLower();
        StringBuilder builder = new StringBuilder(text);
        for(int i = 0; i < builder.Length; i++) {
            if(i == 0) {
                builder[0] = char.ToUpper(builder[0]);
            } else if (builder[i] == ' ') {
                builder[i + 1] = char.ToUpper(builder[i + 1]);
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// input < min
    /// min * multiplier_1
    /// 
    /// input <= break_point_1
    /// input * multiplier_1
    /// 
    /// input > break_point && input <= max
    /// break_point * multiplier_1 + (input - break_point) * multiplier_2
    /// 
    /// input > max
    /// break_point * multiplier_1 + (max - break_point) * multiplier_2
    /// </summary>
    /// <param name="min"></param>
    /// <param name="break_point_1"></param>
    /// <param name="max"></param>
    /// <param name="multiplier_1"></param>
    /// <param name="multiplier_2"></param>
    /// <returns></returns>
    public static float Break_Point_Multiply(float input, float min, float break_point, float max, float multiplier_1, float multiplier_2)
    {
        if(break_point <= min || break_point >= max) {
            Logger.Instance.Warning("Invalid input!");
            return -1.0f;
        }
        if(input < min) {
            return min * multiplier_1;
        }
        if(input <= break_point) {
            return input * multiplier_1;
        }
        if(input > break_point && input <= max) {
            return (break_point * multiplier_1) + ((input - break_point) * multiplier_2);
        }
        return (break_point * multiplier_1) + ((max - break_point) * multiplier_2);
    }
}
