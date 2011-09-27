/* 
 * Name: Queries.cs
 * Programmed by: Karthik Abiraman (kmabiraman@gmail.com)
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace WorldTime
{
    /// <summary>
    /// This class contains all the db queries that are required by the 
    /// WorldTime application.
    /// </summary>
    static class Queries
    {
        public static string GetMyCitiesCount()
        {
            return "SELECT COUNT(1) FROM Saved_City";
        }

        public static string GetMyCities()
        {
            return "SELECT City_ID, Ct.Name, St.Name, Coun.Name, " +
                "T.Name, T.Offset, T.DST, T.DST_Start, T.DST_End " + 
                "FROM Timezone T, City Ct " + 
                ", State St, Country Coun WHERE " + 
                " T.Timezone_ID = Ct.Timezone_ID AND " + 
                "Ct.State_ID = St.State_ID " + 
                "AND St.Country_ID = Coun.Country_ID AND City_ID IN " +
            "(SELECT City_ID FROM Saved_City) ORDER BY Ct.Name";
        }

        public static string GetAllCitiesCount()
        {
            return "SELECT COUNT(1) FROM City";
        }

        public static string GetAllCities()
        {
            return "SELECT C.City_ID, C.Name, St.Name, Con.Name FROM City C, State " +
            "St, Country Con WHERE C.State_ID = St.State_ID AND St.Country_ID" + 
            " = Con.Country_ID ORDER BY C.Name";
        }

        public static string GetContinentCount()
        {
            return "SELECT COUNT(1) FROM Continent";
        }

        public static string GetAllContinents()
        {
            return "SELECT * FROM Continent ORDER BY Name";
        }

        public static string GetCountryCount(int intContinentID)
        {
            return "SELECT COUNT(1) FROM Country WHERE Continent_ID = " 
                + intContinentID.ToString();
        }

        public static string GetCountryByContinent(int intContinentID)
        {
            return "SELECT Country_ID, Name FROM Country WHERE Continent_ID = " +
                intContinentID.ToString() + " ORDER BY Name";
        }

        public static string GetStateCount(int intCountryID)
        {
            return "SELECT COUNT(1) FROM State WHERE Country_ID = "
                + intCountryID.ToString();
        }

        public static string GetStatesByCountry(int intCountryID)
        {
            return "SELECT State_ID, Name FROM State WHERE Country_ID = " +
                intCountryID.ToString() + " ORDER BY Name";
        }
        
        public static string GetCityByCountryCount(int intCountryID)
        {
            return "SELECT COUNT(1) FROM City WHERE State_ID IN (Select " +
                "State_ID FROM State WHERE Country_ID = "
                + intCountryID.ToString() + ")";
        }

        public static string GetCityByCountry(int intCountryID)
        {
            return "SELECT City_ID, Name FROM City WHERE State_ID IN (Select " +
                "State_ID FROM State WHERE Country_ID = "
                + intCountryID.ToString() + ") ORDER BY Name";
        }

        public static string GetCityCount(int intStateID)
        {
            return "SELECT COUNT(1) FROM City WHERE State_ID = "
                + intStateID.ToString();
        }

        public static string GetCityByState(int intStateID)
        {
            return "SELECT City_ID, Name FROM City WHERE State_ID = " +
                intStateID.ToString() + " ORDER BY Name";
        }
        
        public static string GetCityDetails(int intCityID)
        {
            return "SELECT T.Name, T.Offset, T.DST, T.DST_Start, T.DST_End, " + 
            "C.Name, C.Is_Country_Capital, Coun.Phone_Code, C.Phone_Code, " +
            "C.Weather_Code, S.Name, Coun.Name, Con.Name FROM City C, Timezone T, Country Coun, State S, " + 
            "Continent Con WHERE C.Timezone_ID = T.Timezone_ID AND C.State_ID " +
            "= S.State_ID AND S.Country_ID = Coun.Country_ID AND Coun.Continent_ID = Con.Continent_ID AND C.City_ID = "
            + intCityID.ToString();
        }

        public static string CheckIfCityExists(int intCityID)
        {
            return "SELECT COUNT(1) FROM Saved_City WHERE City_ID = " +
                intCityID.ToString();
        }
        
        public static string AddToMyCities(int intCityID)
        {
            return "INSERT INTO Saved_City(City_ID) VALUES(" + 
                intCityID.ToString() + ")";
        }

        public static string DeleteFromMyCities(int intCityID)
        {
            return "DELETE FROM Saved_City WHERE City_ID = " +
                intCityID.ToString();
        }

        public static string FindCity(string strCity)
        {
            return "SELECT City_ID FROM City WHERE LOWER(Name) = LOWER('"
            + strCity + "')";
        }
    }
}