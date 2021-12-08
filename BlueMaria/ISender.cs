namespace BlueMaria
{
    public interface ISender
    {
        //void PressKey(short k);
        //void ReleaseKey(short k);
        //int SendCommand(string[] r);
        //void SendKey(short k);
        //void SendPhrase(string s);
        void SendPhrase(string s, ref bool noSpace);
        //int SendSymbol(string[] r);
        //void SendWord(string s);
    }

}