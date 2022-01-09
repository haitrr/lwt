package main

import (
	"fmt"
	"net/http"
	"os"

	"github.com/go-ego/gse"
	echo "github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
)

var seg gse.Segmenter

type request struct {
	Text string `json:"text"`
}

func cut(c echo.Context) error {
	var r request
	if err := c.Bind(&r); err != nil {
		return err
	}

	result := seg.Cut(r.Text, true)
	c.JSON(http.StatusOK, result)
	return nil
}

func main() {
	e := echo.New()

	err := seg.LoadDict("jp")
	if err != nil {
		fmt.Println("failed to load dict")
		return
	}
	e.Use(middleware.Logger())
	e.Use(middleware.Recover())

	e.GET("/", func(c echo.Context) error {
		return c.HTML(http.StatusOK, "Hello, Docker! <3")
	})
	e.POST("/cut", cut)

	e.GET("/ping", func(c echo.Context) error {
		return c.JSON(http.StatusOK, struct{ Status string }{Status: "OK"})
	})

	httpPort := os.Getenv("HTTP_PORT")
	if httpPort == "" {
		httpPort = "8080"
	}

	e.Logger.Fatal(e.Start(":" + httpPort))
}
