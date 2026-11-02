public class Item
{
    public string m_name;
    public int m_durability;

    public Item(string name, int durability)
    {
        m_name = name;
        m_durability = durability;
    }
    public bool ReduceDurability()
    {
        m_durability--;
        if (m_durability == 0)
        {
            return false;
        }
        return true;
    }
}
